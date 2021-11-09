// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2Bridge.Tests.End2End
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using FluentAssertions.Json;
    using Kusto.Data.Common;
    using Kusto.Data.Ingestion;
    using Kusto.Data.Net.Client;
    using Newtonsoft.Json.Linq;
    using NUnit.Framework;

    public class KustoClientTests : KustoTestBase
    {
        protected const string TypesDir = "../../../types";

        /// <summary>
        /// Name of the Kusto index for types check.
        /// </summary>
        protected const string TypesIndex = "types_index";

        protected const string TypesMapping = "types_mapping";

        // Map Kusto columns to types
        private static readonly Dictionary<string, string> KustoColumnType = new Dictionary<string, string> {
                { "Boolean", "bool" },
                { "DateTime", "datetime" },
                { "Guid", "guid" },
                { "Int32", "int" },
                { "Int64", "long" },
                { "Double", "real" },
                { "String", "string" },
                { "TimeSpan", "timespan" },
                { "SqlDecimal", "decimal" },
                { "Dynamic", "dynamic" },
            };

        private static ICslAdminProvider kustoAdminClient;

        private static string typesIndexFullName;

        [OneTimeSetUp]
        public static void CreateClient()
        {
            using (kustoAdminClient = KustoClientFactory.CreateCslAdminProvider(Kusto()))
            {
                PopulateTypesIndexData();
            }

            typesIndexFullName = $"{KustoDatabase()}:{TypesIndex}";
        }

        [Test]
        [Description("MSearch returns all data types as expected")]
        public async Task MSearch_All_ReturnsAllHitsAsExpected()
        {
            var result = await K2Client().MSearch(TypesIndex, $"{TypesDir}/MSearch_All_InTimeRange.json");
            var totalHits = result.SelectToken("responses[0].hits.total.value");
            Assert.IsNotNull(totalHits);
            Assert.IsTrue(totalHits.Value<int>() == 1);

            var firstHit = result.SelectToken("responses[0].hits.hits[0]");
            var expectedHit = JObject.Parse(@"{
                    ""_source"": {
                        ""Boolean"": true,
                        ""DateTime"": ""2020-02-23T07:22:29.1990163"",
                        ""Guid"": ""74be27de-1e4e-49d9-b579-fe0b331d3642"",
                        ""Int32"": 17,
                        ""Int64"": 17,
                        ""Double"": 0.3,
                        ""String"": ""string type"",
                        ""TimeSpan"": ""PT30M"",
                        ""SqlDecimal"": 0.3,
                        ""Dynamic"": {
                            ""a"": 123,
                            ""b"": ""hello""
                        }
                    }
                }");

            firstHit.Should().ContainSubtree(expectedHit);
        }

        [Test]
        [Description("FieldCaps returns fields for all types as expected")]
        public async Task FieldCaps_WithAllTypes_ReturnsTypesConvertedToESTypes()
        {
            var fieldCaps = await K2Client().FieldCaps(typesIndexFullName);
            var expected = JObject.Parse(@"{
              ""indices"": [
                ""types_index""
              ],
              ""fields"": {
                ""Boolean"": {
                  ""boolean"": {
                    ""aggregatable"": true,
                    ""searchable"": true,
                    ""type"": ""boolean""
                  }
                },
                ""DateTime"": {
                  ""date"": {
                    ""aggregatable"": true,
                    ""searchable"": true,
                    ""type"": ""date""
                  }
                },
                ""Guid"": {
                  ""string"": {
                    ""aggregatable"": true,
                    ""searchable"": true,
                    ""type"": ""string""
                  }
                },
                ""Int32"": {
                  ""integer"": {
                    ""aggregatable"": true,
                    ""searchable"": true,
                    ""type"": ""integer""
                  }
                },
                ""Int64"": {
                  ""long"": {
                    ""aggregatable"": true,
                    ""searchable"": true,
                    ""type"": ""long""
                  }
                },
                ""Double"": {
                  ""double"": {
                    ""aggregatable"": true,
                    ""searchable"": true,
                    ""type"": ""double""
                  }
                },
                ""String"": {
                  ""keyword"": {
                    ""aggregatable"": true,
                    ""searchable"": true,
                    ""type"": ""keyword""
                  }
                },
                ""TimeSpan"": {
                  ""string"": {
                    ""aggregatable"": true,
                    ""searchable"": true,
                    ""type"": ""string""
                  }
                },
                ""SqlDecimal"": {
                  ""double"": {
                    ""aggregatable"": true,
                    ""searchable"": true,
                    ""type"": ""double""
                  }
                },
                ""Dynamic"": {
                  ""object"": {
                    ""aggregatable"": true,
                    ""searchable"": true,
                    ""type"": ""object""
                  }
                },
              }
            }");
            fieldCaps.Should().BeEquivalentTo(expected);
        }

        private static void PopulateTypesIndexData()
        {
            // Build list of columns and mappings to provision Kusto
            var kustoColumns = new List<string>();
            var columnMappings = new List<ColumnMapping>();

            foreach (var entry in KustoColumnType)
            {
              var name = entry.Key;
              kustoColumns.Add($"{name}:{entry.Value}");
              columnMappings.Add(new ColumnMapping()
              {
                ColumnName = name,
                Properties = new Dictionary<string, string>
                {
                  ["Path"] = $"$.{name}",
                },
              });
            }

            // Send drop table ifexists command to Kusto
            var command = CslCommandGenerator.GenerateTableDropCommand(TypesIndex, true);
            KustoExecute(command);

            // Send create table command to Kusto
            command = $".create table {TypesIndex} ({string.Join(", ", kustoColumns)})";
            Console.WriteLine(command);
            KustoExecute(command);

            // Send create table mapping command to Kusto
            command = CslCommandGenerator.GenerateTableMappingCreateCommand(IngestionMappingKind.Json, TypesIndex, TypesMapping, columnMappings, true);
            KustoExecute(command);

            command = ".append types_index <|" +
                "print x = true, datetime('2020-02-23T07:22:29.1990163Z'), guid(74be27de-1e4e-49d9-b579-fe0b331d3642), int(17), long(17), real(0.3), 'string type', 30m, decimal(0.3), dynamic({'a':123, 'b':'hello'})";
            KustoExecute(command);
        }

        private static void KustoExecute(string command)
        {
            TestContext.Progress.WriteLine(command);
            kustoAdminClient.ExecuteControlCommand(command);
        }
    }
}
