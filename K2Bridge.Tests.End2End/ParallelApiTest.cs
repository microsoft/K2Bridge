// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2Bridge.Tests.End2End
{
    using System;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Net;
    using System.Threading.Tasks;
    using FluentAssertions;
    using FluentAssertions.Json;
    using Kusto.Data;
    using Newtonsoft.Json.Linq;
    using NUnit.Framework;

    /// <summary>
    /// Parallel end-to-end tests loading data into Kusto and Elasticsearch and assuring
    /// that K2Bridge returns equivalent outputs to Elasticsearch.
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
    /// By default, <c>https://kustok2bridge.westeurope.kusto.windows.net</c>.
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
    [TestFixture]
    public class ParallelApiTest
    {
        /// <summary>
        /// Name of the Elasticsearch index / Kusto table to create and populate.
        /// </summary>
        private const string INDEX = "kibana_sample_data_flights";

        /// <summary>
        /// Name of the Kusto ingestion mapping to create.
        /// </summary>
        private const string MAPPING = "test_mapping";

        private const string FLIGHTSDIR = "../../../flights";

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
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", Justification = "Parameter context required by test framework")]
        public static async Task Init()
        {
            var kustoUri = Environment.GetEnvironmentVariable("KUSTO_URI") ?? "https://kustok2bridge.westeurope.kusto.windows.net";

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

        [SetUp]
        public async Task SetUp()
        {
            string testMethodName = NUnit.Framework.TestContext.CurrentContext.Test.Name;

            k2Client = await CreateKustoClient(testMethodName);

            esClient = await CreateElasticsearchClient(testMethodName);
        }

        /// <summary>
        /// Ensure the JSON response at the API root (containing general cluster information)
        /// is equivalent.
        /// This is not actually required for K2Bridge functionality,
        /// but is a test for the generic passthrough functionality
        /// to the K2Bridge internal Elasticsearch.
        /// </summary>
        [Test]
        [Description("Cluster general info (at API root URL)")]
        public void ClusterInfo_Equivalent()
        {
            var es = esClient.ClusterInfo();
            var k2 = k2Client.ClusterInfo();
            AssertJsonIdentical(k2.Result, es.Result);
        }

        [Test]
        [Description("/_msearch Kibana aggregation query returning zero results")]
        public void MSearch_ZeroResults_Equivalent()
        {
            ParallelQuery($"{FLIGHTSDIR}/MSearch_ZeroResults_Equivalent.json", minResults: 0);
        }

        [Test]
        [Description("/_msearch Kibana aggregation query returning two results")]
        public void MSearch_TwoResults_Equivalent()
        {
            ParallelQuery($"{FLIGHTSDIR}/MSearch_TwoResults_Equivalent.json");
        }

        [Test]
        [Description("/_msearch Kibana aggregation query with text filter")]
        public void MSearch_TextFilter_Equivalent()
        {
            ParallelQuery(
                $"{FLIGHTSDIR}/MSearch_TextFilter_Equivalent.json");
        }

        [Test]
        [Description("/_msearch Kibana aggregation query with text (includes prefix) filter")]
        public void MSearch_TextFilter_Prefix_Equivalent()
        {
            ParallelQuery(
                $"{FLIGHTSDIR}/MSearch_TextFilter_Prefix_Equivalent.json");
        }

        [Test]
        [Description("/_msearch Kibana aggregation query with text (includes wildcard) filter")]
        public void MSearch_TextFilter_Wildcard_Equivalent()
        {
            ParallelQuery(
                $"{FLIGHTSDIR}/MSearch_TextFilter_Wildcard_Equivalent.json");
        }

        [Test]
        [Description("/_msearch Kibana aggregation query with text (includes specific field) filter")]
        public void MSearch_TextFilter_FieldSpecific_Equivalent()
        {
            ParallelQuery(
                $"{FLIGHTSDIR}/MSearch_TextFilter_FieldSpecific_Equivalent.json");
        }

        [Test]
        [Description("/_msearch Kibana aggregation query with multiple filters")]

        // TODO: fix timezone in bucketing and enable test
        // https://dev.azure.com/csedevil/K2-bridge-internal/_workitems/edit/1479
        // TODO: fix multiple highlights and enable test
        // https://dev.azure.com/csedevil/K2-bridge-internal/_workitems/edit/1481
        [Ignore("Requires fixing issues 1479 and 1481")]
        public void MSearch_MultipleFilters_Equivalent()
        {
            ParallelQuery($"{FLIGHTSDIR}/MSearch_MultipleFilters_Equivalent.json");
        }

        // NB Timestamp sorting is already covered by other test cases
        [Test]
        [TestCase("MSearch_Sort_String.json")]
        [TestCase("MSearch_Sort_Double.json")]
        [Description("/_msearch sort attribute with various data types")]
        public void MSearch_Sort_Equivalent(string queryFileName)
        {
            ParallelQuery($"{FLIGHTSDIR}/{queryFileName}");
        }

        [Test]
        [Description("/_search index list Kibana query")]
        public void Search_Equivalent()
        {
            var es = esClient.Search();
            var k2 = k2Client.Search($"{kustoDatabase}:{INDEX}");
            AssertJsonIdentical(k2.Result, es.Result);
        }

        [Test]
        [Description("/{index}/_field_caps field capabilities Kibana query")]
        public void FieldCaps_Equivalent_WithoutDatabaseName()
        {
            var es = esClient.FieldCaps(INDEX);
            var k2 = k2Client.FieldCaps(INDEX);
            AssertJsonIdentical(k2.Result, es.Result);
        }

        [Test]
        [Description("/{index}/_field_caps field capabilities Kibana query")]
        public void FieldCaps_Equivalent_WithDatabaseName()
        {
            var es = esClient.FieldCaps(INDEX);
            var k2 = k2Client.FieldCaps($"{kustoDatabase}%3A{INDEX}");
            AssertJsonIdentical(k2.Result, es.Result);
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
            var esTask = PopulateElastic.Populate(esClient, INDEX, structure, dataGzippedJsonFile);
            var k2Task = PopulateKusto.Populate(kusto, kustoDatabase, INDEX, MAPPING, structure, "flights.json.gz");
            Task.WaitAll(esTask, k2Task);

            // Log information to console.
            // Can't use Console.WriteLine here: https://github.com/nunit/nunit3-vs-adapter/issues/266
            TestContext.Progress.WriteLine($"Ingestion into Elasticsearch and Kusto completed");
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

        private static void AssertJsonIdentical(JToken k2, JToken es)
        {
            k2.Should().BeEquivalentTo(es);
        }

        private void ParallelQuery(string esQueryFile, string k2QueryFile = null, int minResults = 1)
        {
            var es = esClient.MSearch(INDEX, esQueryFile);
            var k2 = k2Client.MSearch(INDEX, k2QueryFile ?? esQueryFile);
            var t = es.Result.SelectToken("responses[0].hits.total");
            Assert.IsTrue(t.Value<int>() >= minResults);
            AssertJsonIdentical(k2.Result, es.Result);
        }
    }
}
