// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2Bridge.Tests.End2End
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Net;
    using System.Threading.Tasks;
    using Kusto.Data;
    using NUnit.Framework;
    using static System.Threading.Tasks.TaskScheduler;
    using static NUnit.Framework.TestContext;

    /// <summary>
    /// End-to-end tests loading data into Kusto and performing tests against Kusto.
    ///
    /// These integration tests require preexisting resources:
    /// <list type="bullet">
    /// <item>
    /// <description>
    /// A Kusto cluster and database.
    /// </description>
    /// <description>
    /// The K2Bridge helm chart, connected to the Kusto cluster (NB: this includes the K2Bridge internal Elasticsearch for metadata storage, which is not relevant to this test class).
    /// </description>
    /// <description>
    /// The Elasticsearch helm chart, to have an independent Elasticsearch instance for parallel queries.
    /// </description>
    /// </item>
    /// </list>
    /// The tests use the following environment variables:
    /// <list type="bullet">
    /// <item>
    /// <description>
    /// K2BRIDGE_URL: URL to the K2Bridge HTTP endpoint. The CI/CD pipeline sets it
    /// to the AKS service URL (http://k2bridge:8080). In local development, run
    /// <c>kubectl port-forward service/k2bridge 8080</c> so the endpoint is available
    /// at http://localhost:8080 (default value).
    /// </description>
    /// </item>
    /// <item>
    /// <description>
    /// ELASTICSEARCH_URL: URL to the Elasticsearch HTTP endpoint. The CI/CD pipeline sets it
    /// to the AKS service URL (http://elasticsearchqa-master:9200). In local development, run
    /// <c>kubectl port-forward service/elasticsearchqa-master 9200</c>
    /// so the endpoint is available at http://localhost:9200 (default value).
    /// </description>
    /// </item>
    /// <item>
    /// <description>
    /// KUSTO_URI: URL to the Kusto HTTP endpoint.
    /// By default, <c>https://k2devkusto.westeurope.kusto.windows.net</c>.
    /// </description>
    /// </item>
    /// <item>
    /// <description>
    /// KUSTO_DB: Name of the Kusto database to use for test data.
    /// </description>
    /// </item>
    /// <item>
    /// <description>
    /// AAD_TOKEN: Used by CI/CD pipeline to pass an Azure AD token to access Kusto.
    /// In local development, can be omitted, the script will then use the Azure CLI
    /// to generate a token.
    /// </description>
    /// </item>
    /// </list>
    /// </summary>
    public abstract class KustoTestBase
    {
        /// <summary>
        /// Name of the Elasticsearch index / Kusto table to create and populate.
        /// </summary>
        protected const string INDEX = "kibana_sample_data_flights";

        /// <summary>
        /// Name of the Kusto ingestion mapping to create.
        /// </summary>
        protected const string MAPPING = "test_mapping";

        protected const string FLIGHTSDIR = "../../../flights";

        /// <summary>
        ///  Name of the Kusto database to populate with test data.
        /// </summary>
        private static string kustoDatabase;

        /// <summary>
        /// Factory instance for Kusto management and ingestion clients.
        /// </summary>
        private static KustoConnectionStringBuilder kusto;

        /// <summary>
        /// Client instance configured to connect to the K2 backend.
        /// </summary>
        private TestElasticClient k2Client;

        /// <summary>
        /// Client instance configured to connect to the Elasticsearch backend.
        /// </summary>
        private TestElasticClient esClient;

        /// <summary>
        /// Test class initializer populating the Kusto and Elasticsearch parallel backends
        /// with identical data.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        [OneTimeSetUp]
        public static async Task Init()
        {
            var kustoUri = Environment.GetEnvironmentVariable("KUSTO_URI") ?? "https://k2devkusto.westeurope.kusto.windows.net";

            kustoDatabase = Environment.GetEnvironmentVariable("KUSTO_DB") ?? "test";

            var token = Environment.GetEnvironmentVariable("AAD_TOKEN") ?? GetAADToken(kustoUri);

            kusto = new KustoConnectionStringBuilder(kustoUri, kustoDatabase)
               .WithAadApplicationTokenAuthentication(token);

            if (!File.Exists("flights.json.gz"))
            {
                using var wc = new WebClient();
                wc.DownloadFile("https://raw.githubusercontent.com/elastic/kibana/v6.8.5/src/server/sample_data/data_sets/flights/flights.json.gz", "flights.json.gz");
            }

            await PopulateBothBackends($"{FLIGHTSDIR}/structure.json", "flights.json.gz");
        }

        /// <summary>
        ///  Name of the Kusto database to populate with test data.
        /// </summary>
        [SetUp]
        public async Task SetUp()
        {
            string testMethodName = CurrentContext.Test.Name;

            k2Client = await CreateKustoClient(testMethodName);

            esClient = await CreateElasticsearchClient(testMethodName);
        }

        protected static string KustoDatabase() {
            return kustoDatabase;
        }

        protected static KustoConnectionStringBuilder Kusto() {
            return kusto;
        }

        protected TestElasticClient K2Client() {
            return k2Client;
        }

        protected TestElasticClient ESClient() {
            return esClient;
        }

        /// <summary>
        /// Utility method for local development. Assuming the current user is logged
        /// in with a service principal that is authorized to write to the Kusto database,
        /// calls 'az account get-access-token' to retrieve an AAD token to connect
        /// to Kusto.
        /// </summary>
        /// <param name="uri">Resource URI to get an AAD token for.</param>
        /// <returns>An AAD access token.</returns>
        private static string GetAADToken(string uri)
        {
            // Create process
            using var az = new Process();
            az.StartInfo.FileName = "az";
            az.StartInfo.Arguments = $"account get-access-token --query accessToken -o tsv --resource {uri}";
            az.StartInfo.UseShellExecute = false;
            az.StartInfo.RedirectStandardOutput = true;
            az.Start();
            string strOutput = az.StandardOutput.ReadToEnd();
            az.WaitForExit(10000);
            if (az.ExitCode != 0)
            {
                throw new Exception("Error running az account get-access-token");
            }

            return strOutput.Trim();
        }

        private static async Task PopulateBothBackends(string structureJsonFile, string dataGzippedJsonFile)
        {
            var structure = File.ReadAllText(structureJsonFile);
            var esClient = await CreateElasticsearchClient("init");

            // For immediately logging information to console, use Progress.WriteLine.
            // Can't use Console.WriteLine: https://github.com/nunit/nunit3-vs-adapter/issues/266
            var esTask = PopulateElastic.Populate(esClient, INDEX, structure, dataGzippedJsonFile).ContinueWith(
                (t) => Progress.WriteLine($"Ingestion into Elasticsearch completed"), Current);
            var k2Task = PopulateKusto.Populate(kusto, kustoDatabase, INDEX, MAPPING, structure, "flights.json.gz").ContinueWith(
                (t) => Progress.WriteLine($"Ingestion into Kusto completed"), Current);
            Task.WaitAll(esTask, k2Task);
        }

        private static async Task<TestElasticClient> CreateElasticsearchClient(string prefix)
        {
            var esUri = Environment.GetEnvironmentVariable("ELASTICSEARCH_URL") ?? "http://localhost:9200";
            return await TestElasticClient.Create(esUri, $"{prefix}-es.json");
        }

        private static async Task<TestElasticClient> CreateKustoClient(string prefix)
        {
            var bridgeUri = Environment.GetEnvironmentVariable("K2BRIDGE_URL") ?? "http://localhost:8080";
            return await TestElasticClient.Create(bridgeUri, $"{prefix}-k2.json");
        }
    }
}
