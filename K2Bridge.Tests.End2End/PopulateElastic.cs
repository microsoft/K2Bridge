// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2Bridge.Tests.End2End
{
    using System.IO;
    using System.IO.Compression;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Text;
    using System.Threading.Tasks;
    using Newtonsoft.Json.Linq;
    using NUnit.Framework;

    /// <summary>
    /// Utility class to load test data into Elasticsearch.
    /// </summary>
    public static class PopulateElastic
    {
        /// <summary>
        ///  Populate the Elasticsearch backend with test data.
        /// </summary>
        /// <param name="esClient">Test client instance configured to connect to Elasticsearch.</param>
        /// <param name="index">Name of the Elasticsearch index to create.</param>
        /// <param name="structure">JSON file containing the Elasticsearch index structure.</param>
        /// <param name="dataFile">Gzipped JSON file containing the data to be loaded.</param>
        /// <returns>Bulk Insert operation result.</returns>
        public static async Task<JToken> Populate(TestElasticClient esClient, string index, string structure, string dataFile)
        {
            // Create index
            _ = await CreateIndex(esClient, index, structure);

            // Log information to console.
            // Can't use Console.WriteLine here: https://github.com/nunit/nunit3-vs-adapter/issues/266
            TestContext.Progress.WriteLine($"Decompressing {dataFile} and bulk inserting content into Elasticsearch");

            // Populate index
            using Stream fs = File.OpenRead(dataFile);
            using var decompressionStream = new GZipStream(fs, CompressionMode.Decompress);
            using var reader = new StreamReader(decompressionStream);
            return await BulkInsert(esClient, index, reader);
        }

        /// <summary>
        /// API operation to create an index. Also deletes the index before creating it, if it already exists.
        /// </summary>
        /// <param name="client"></param>
        /// <param name="indexName">Index to be created.</param>
        /// <param name="structureJson">JSON definition of the Elasticsearch index.</param>
        /// <returns>Create index operation result.</returns>
        public static async Task<JToken> CreateIndex(TestElasticClient client, string indexName, string structureJson)
        {
            // Delete Elasticsearch index if it exists (for idempotent runs)
            using (var drequest = new HttpRequestMessage(HttpMethod.Delete, indexName))
            {
                var dresult = client.JsonQuery(drequest);
            }

            // Create Elasticsearch index and define mappings
            using var request = new HttpRequestMessage(HttpMethod.Put, indexName + "?include_type_name=true")
            {
                Content = new StringContent(structureJson),
            };
            request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");
            return await client.JsonQuery(request);
        }

        /// <summary>
        /// API operation to insert multiple documents into an index.
        /// </summary>
        /// <param name="indexName">Index where data is to be inserted.</param>
        /// <param name="reader">A stream containing JSON documents, one per line.</param>
        /// <returns>Bulk Insert operation result.</returns>
        private static async Task<JToken> BulkInsert(TestElasticClient client, string indexName, StreamReader reader)
        {
            // Change data to format required by Bulk Insert API (pairs of lines with index definition and data)
            var ndJson = new StringBuilder();
            string line;
            while ((line = reader.ReadLine()) != null)
            {
                ndJson.AppendLine("{\"index\":{}}");
                ndJson.AppendLine(line);
            }

            // Bulk insert data
            using var request = new HttpRequestMessage(HttpMethod.Post, $"{indexName}/_doc/_bulk")
            {
                Content = new StringContent(ndJson.ToString()),
            };
            request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/x-ndjson");
            var result = await client.JsonQuery(request);
            var hasErrors = result.SelectToken("errors") as JValue;
            Assert.IsFalse((bool)hasErrors.Value, "{0}", result);
            return result;
        }
    }
}
