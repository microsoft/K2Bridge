// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2Bridge.Tests.End2End
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.IO;
    using System.Threading.Tasks;
    using Kusto.Data;
    using Kusto.Data.Common;
    using Kusto.Data.Net.Client;
    using Kusto.Ingest;
    using Newtonsoft.Json.Linq;
    using NUnit.Framework;

    /// <summary>
    /// Utility class to load test data into Kusto.
    /// </summary>
    public static class PopulateKusto
    {
        // Map from Elasticsearch type to Kusto type, when type names differ
        private static readonly Dictionary<string, string> ES2KUSTOTYPE = new Dictionary<string, string> {
                { "text", "string" },
                { "keyword", "string" },
                { "float", "double" },
                { "integer", "int" },
                { "geo_point", "dynamic" },
            };

        /// <summary>
        ///  Populate the Kusto backend with test data.
        /// </summary>
        /// <param name="kusto"></param>
        /// <param name="db"></param>
        /// <param name="table"></param>
        /// <param name="mapping"></param>
        /// <param name="structure">JSON file containing the Elasticsearch index structure. Elasticsearch types will be converted to Kusto types. Note that the method only supported a small set of Elasticsearch types.</param>
        /// <param name="dataFile">Gzipped JSON file containing the data to be loaded.</param>
        /// <returns>Bulk Insert operation result.</returns>
        public static async Task<IKustoIngestionResult> Populate(KustoConnectionStringBuilder kusto, string db, string table, string mapping, string structure, string dataFile)
        {
            var struc = JObject.Parse(structure);
            var properties = struc["mappings"]["_doc"]["properties"] as JObject;

            // Build list of columns and mappings to provision Kusto
            var kustoColumns = new List<string>();
            var columnMappings = new List<JsonColumnMapping>();
            foreach (var prop in properties)
            {
                string name = prop.Key;
                JObject value = prop.Value as JObject;
                string type = (string)value["type"];
                if (ES2KUSTOTYPE.ContainsKey(type))
                {
                    type = ES2KUSTOTYPE[type];
                }

                kustoColumns.Add($"{name}:{type}");
                columnMappings.Add(new JsonColumnMapping()
                { ColumnName = name, JsonPath = $"$.{name}" });
            }

            using (var kustoAdminClient = KustoClientFactory.CreateCslAdminProvider(kusto))
            {
                // Send drop table ifexists command to Kusto
                var command = CslCommandGenerator.GenerateTableDropCommand(table, true);
                kustoAdminClient.ExecuteControlCommand(command);

                // Send create table command to Kusto
                command = $".create table {table} ({string.Join(", ", kustoColumns)})";
                Console.WriteLine(command);
                kustoAdminClient.ExecuteControlCommand(command);

                // Send create table mapping command to Kusto
                command = CslCommandGenerator.GenerateTableJsonMappingCreateCommand(
                                                    table, mapping, columnMappings);
                kustoAdminClient.ExecuteControlCommand(command);
            }

            // Log information to console.
            // Can't use Console.WriteLine here: https://github.com/nunit/nunit3-vs-adapter/issues/266
            TestContext.Progress.WriteLine($"Ingesting {dataFile} as compressed data into Kusto");

            // Populate Kusto
            using Stream fs = File.OpenRead(dataFile);
            return await KustoIngest(kusto, db, table, mapping, fs);
        }

        /// <summary>
        /// Ingest data into Kusto.
        /// </summary>
        /// <param name="table">Name of table to ingest into.</param>
        /// <param name="mappingName">Name of table mapping to ingest with.</param>
        /// <param name="stream">input JSON data stream.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        private static async Task<IKustoIngestionResult> KustoIngest(KustoConnectionStringBuilder kusto, string db, string table, string mappingName, Stream stream)
        {
            // Create a disposable client that will execute the ingestion
            using IKustoIngestClient client = KustoIngestFactory.CreateDirectIngestClient(kusto);
            var ingestProps = new KustoIngestionProperties(db, table)
            {
                JSONMappingReference = mappingName,
                Format = DataSourceFormat.json,
            };
            var ssOptions = new StreamSourceOptions
            {
                CompressionType = DataSourceCompressionType.GZip,
            };

            return await client.IngestFromStreamAsync(stream, ingestProps, ssOptions);
        }
    }
}
