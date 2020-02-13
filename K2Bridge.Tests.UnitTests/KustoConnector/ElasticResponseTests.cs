// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace UnitTests.K2Bridge.KustoConnector
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Linq;
    using global::K2Bridge.KustoConnector;
    using global::K2Bridge.Models;
    using global::K2Bridge.Models.Response;
    using Microsoft.Extensions.Logging;
    using Moq;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using NUnit.Framework;

    [TestFixture]
    public class ElasticResponseTests
    {
        private const string HitTestId = "999";

        private QueryData query = new QueryData("_kql", "_index", null);

        [TestCase(ExpectedResult = "{\"_index\":\"_index\",\"_type\":\"_doc\",\"_id\":\"999\",\"_version\":1,\"_score\":null,\"_source\":{\"somefield1\":\"somevalue1\",\"somefield2\":\"somevalue2\",\"somefield3\":\"somevalue3\"},\"fields\":{},\"sort\":[],\"highlight\":{\"somefield1\":[\"hlsomevalue1/hl\"]}}")]
        public string MapRowsToHits_WhenSingleWordInQueryString_HighlightIsIsValid()
        {
            return MakeHighlightHit(
                new Dictionary<string, object> {
                        { "somefield1", "somevalue1" },
                        { "somefield2", "somevalue2" },
                        { "somefield3", "somevalue3" },
                    },
                "*",
                "somevalue1");
        }

        [TestCase(ExpectedResult = "{\"_index\":\"_index\",\"_type\":\"_doc\",\"_id\":\"999\",\"_version\":1,\"_score\":null,\"_source\":{\"somefield1\":\"somevalue1\",\"somefield2\":\"somevalue1\",\"somefield3\":\"somevalue3\"},\"fields\":{},\"sort\":[],\"highlight\":{\"somefield1\":[\"hlsomevalue1/hl\"],\"somefield2\":[\"hlsomevalue1/hl\"]}}")]
        public string MapRowsToHits_WhenSingleWordTwoFieldsInQueryString_HighlightIsIsValid()
        {
            return MakeHighlightHit(
                new Dictionary<string, object> {
                        { "somefield1", "somevalue1" },
                        { "somefield2", "somevalue1" },
                        { "somefield3", "somevalue3" },
                    },
                "*",
                "somevalue1");
        }

        [TestCase(ExpectedResult = "{\"_index\":\"_index\",\"_type\":\"_doc\",\"_id\":\"999\",\"_version\":1,\"_score\":null,\"_source\":{\"somefield1\":\"pre somevalue1 post\",\"somefield2\":\"somevalue2\",\"somefield3\":\"somevalue3\"},\"fields\":{},\"sort\":[],\"highlight\":{\"somefield1\":[\"pre hlsomevalue1/hl post\"]}}")]
        public string MapRowsToHits_WhenSingleWordSubstringInQueryString_HighlightIsIsValid()
        {
            return MakeHighlightHit(
                new Dictionary<string, object> {
                        { "somefield1", "pre somevalue1 post" },
                        { "somefield2", "somevalue2" },
                        { "somefield3", "somevalue3" },
                    },
                "*",
                "somevalue1");
        }

        [TestCase(ExpectedResult = "{\"_index\":\"_index\",\"_type\":\"_doc\",\"_id\":\"999\",\"_version\":1,\"_score\":null,\"_source\":{\"somefield1\":\"somevalue1\",\"somefield2\":\"somevalue2\",\"somefield3\":\"somevalue3\"},\"fields\":{},\"sort\":[],\"highlight\":{\"somefield1\":[\"hlsomevalue1/hl\"],\"somefield2\":[\"hlsomevalue2/hl\"]}}")]
        public string MapRowsToHits_WhenMultiWordInQueryString_HighlightIsIsValid()
        {
            return MakeHighlightHit(
                new Dictionary<string, object> {
                        { "somefield1", "somevalue1" },
                        { "somefield2", "somevalue2" },
                        { "somefield3", "somevalue3" },
                    },
                "*",
                "somevalue1 somevalue2");
        }

        [TestCase(ExpectedResult = "{\"_index\":\"_index\",\"_type\":\"_doc\",\"_id\":\"999\",\"_version\":1,\"_score\":null,\"_source\":{\"somefield1\":\"pre somevalue1 post\",\"somefield2\":\"pre somevalue2 post\",\"somefield3\":\"pre somevalue1 post pre somevalue2 post\"},\"fields\":{},\"sort\":[],\"highlight\":{\"somefield1\":[\"pre hlsomevalue1/hl post\"],\"somefield2\":[\"pre hlsomevalue2/hl post\"],\"somefield3\":[\"pre hlsomevalue1/hl post pre hlsomevalue2/hl post\"]}}")]
        public string MapRowsToHits_WhenMultiWordSubstringInQueryString_HighlightIsIsValid()
        {
            return MakeHighlightHit(
                new Dictionary<string, object> {
                        { "somefield1", "pre somevalue1 post" },
                        { "somefield2", "pre somevalue2 post" },
                        { "somefield3", "pre somevalue1 post pre somevalue2 post" },
                    },
                "*",
                "somevalue1 somevalue2");
        }

        [TestCase(ExpectedResult = "{\"_index\":\"_index\",\"_type\":\"_doc\",\"_id\":\"999\",\"_version\":1,\"_score\":null,\"_source\":{\"somefield1\":\"pre SOmevalue1 post\",\"somefield2\":\"pre SOmevalue2 post\",\"somefield3\":\"pre SOmevalue1 post pre SOmevalue2 post\"},\"fields\":{},\"sort\":[],\"highlight\":{\"somefield1\":[\"pre hlSOmevalue1/hl post\"],\"somefield2\":[\"pre hlSOmevalue2/hl post\"],\"somefield3\":[\"pre hlSOmevalue1/hl post pre hlSOmevalue2/hl post\"]}}")]
        public string MapRowsToHits_WhenMultiWordDifferentCapsCapialLettersInQueryString_HighlightIsIsValid()
        {
            return MakeHighlightHit(
                new Dictionary<string, object> {
                        { "somefield1", "pre SOmevalue1 post" },
                        { "somefield2", "pre SOmevalue2 post" },
                        { "somefield3", "pre SOmevalue1 post pre SOmevalue2 post" },
                    },
                "*",
                "somevalue1 somevalue2");
        }

        [TestCase(ExpectedResult =
            "{\"responses\":[{\"aggregations\":{\"2\":{\"buckets\":[]}},\"took\":0,\"timed_out\":false,\"_shards\":{\"total\":1,\"successful\":1,\"skipped\":0,\"failed\":0},\"hits\":{\"total\":0,\"max_score\":null,\"hits\":[]},\"status\":200}]}")]
        public string ElasticResponseConstructor_OnValidInput_CreatesValidResponse()
        {
            var defaultResponse = new ElasticResponse();
            var serializedResponse = JsonConvert.SerializeObject(defaultResponse);

            return serializedResponse;
        }

        [TestCase(ExpectedResult = "{\"_index\":\"_index\",\"_type\":\"_doc\",\"_id\":\"999\",\"_version\":1,\"_score\":null,\"_source\":{},\"fields\":{},\"sort\":[]}")]
        public string MapRowsToHits_WhenEmptyHit_HasExpectedElasticProperties()
        {
            var results =
                new List<Dictionary<string, object>>() {
                    new Dictionary<string, object> {
                    },
                };
            var response = BuildHits(results, query);

            return JsonConvert.SerializeObject(SetRandomProperties(response).First());
        }

        [TestCase(ExpectedResult = "{\"_index\":\"_index\",\"_type\":\"_doc\",\"_id\":\"999\",\"_version\":1,\"_score\":null,\"_source\":{\"somefield1\":\"somevalue1\",\"somefield2\":\"somevalue2\",\"somefield3\":\"somevalue3\"},\"fields\":{},\"sort\":[]}")]
        public string MapRowsToHits_WhenSingleHit_HasHasAllFieldsInSource()
        {
            var results =
                new List<Dictionary<string, object>>() {
                    new Dictionary<string, object> {
                        { "somefield1", "somevalue1" },
                        { "somefield2", "somevalue2" },
                        { "somefield3", "somevalue3" },
                    },
                };
            var response = BuildHits(results, query);
            return JsonConvert.SerializeObject(SetRandomProperties(response).First());
        }

        [TestCase(ExpectedResult = new[] {
            "{\"_index\":\"_index\",\"_type\":\"_doc\",\"_id\":\"999\",\"_version\":1,\"_score\":null,\"_source\":{\"somefield1\":\"somevalue1\",\"somefield2\":\"somevalue2\",\"somefield3\":\"somevalue3\"},\"fields\":{},\"sort\":[]}",
            "{\"_index\":\"_index\",\"_type\":\"_doc\",\"_id\":\"999\",\"_version\":1,\"_score\":null,\"_source\":{\"somefield1\":\"somevalue4\",\"somefield2\":\"somevalue5\",\"somefield3\":\"somevalue6\"},\"fields\":{},\"sort\":[]}", })]
        public string[] MapRowsToHits_WhenMultipleHits_HasHasAllFieldsInSource()
        {
            var results =
                new List<Dictionary<string, object>>() {
                    new Dictionary<string, object> {
                        { "somefield1", "somevalue1" },
                        { "somefield2", "somevalue2" },
                        { "somefield3", "somevalue3" },
                    },
                    new Dictionary<string, object> {
                        { "somefield1", "somevalue4" },
                        { "somefield2", "somevalue5" },
                        { "somefield3", "somevalue6" },
                    },
            };
            var response = BuildHits(results, query);

            return SetRandomProperties(response).Select(r => JsonConvert.SerializeObject(r)).ToArray();
        }

        [TestCase(ExpectedResult = JTokenType.Float)]
        public JTokenType MapRowsToHits_WhenDecimal_ReadByType()
        {
            var results =
                new List<Dictionary<string, object>>() {
                    new Dictionary<string, object> {
                        { "somefield1", 2M },
                    },
            };
            var response = BuildHits(results, query);
            return response.First().Source.GetValue("somefield1", StringComparison.OrdinalIgnoreCase).Type;
        }

        [TestCase(ExpectedResult = JTokenType.Boolean)]
        public JTokenType MapRowsToHits_WhenSByte_ReadBoolean()
        {
            var results =
                new List<Dictionary<string, object>>() {
                    new Dictionary<string, object> {
                        { "somefield1", (sbyte)0 },
                    },
            };
            var response = BuildHits(results, query);
            return response.First().Source.GetValue("somefield1", StringComparison.OrdinalIgnoreCase).Type;
        }

        [TestCase(ExpectedResult = JTokenType.Boolean)]
        public JTokenType MapRowsToHits_WhenBoolean_ReadBoolean()
        {
            var results =
                new List<Dictionary<string, object>>() {
                    new Dictionary<string, object> {
                        { "somefield1", false },
                    },
            };
            var response = BuildHits(results, query);
            return response.First().Source.GetValue("somefield1", StringComparison.OrdinalIgnoreCase).Type;
        }

        [TestCase(ExpectedResult = JTokenType.Integer)]
        public JTokenType MapRowsToHits_WhenInteger_ReadInteger()
        {
            var results =
                new List<Dictionary<string, object>>() {
                    new Dictionary<string, object> {
                        { "somefield1", 1 },
                    },
            };
            var response = BuildHits(results, query);
            return response.First().Source.GetValue("somefield1", StringComparison.OrdinalIgnoreCase).Type;
        }

        [TestCase(ExpectedResult = JTokenType.String)]
        public JTokenType MapRowsToHits_WhenDate_ReadString()
        {
            var results =
                new List<Dictionary<string, object>>() {
                    new Dictionary<string, object> {
                        { "somefield1", DateTime.Now },
                    },
            };
            var response = BuildHits(results, query);
            return response.First().Source.GetValue("somefield1", StringComparison.OrdinalIgnoreCase).Type;
        }

        [TestCase(ExpectedResult = new[] {
            "{\"_index\":\"_index\",\"_type\":\"_doc\",\"_id\":\"999\",\"_version\":1,\"_score\":null,\"_source\":{\"somefield1\":\"2020-01-10T16:27:51.3640182\"},\"fields\":{},\"sort\":[]}",
            "{\"_index\":\"_index\",\"_type\":\"_doc\",\"_id\":\"999\",\"_version\":1,\"_score\":null,\"_source\":{\"somefield1\":\"2017-01-02T13:45:23.133\"},\"fields\":{},\"sort\":[]}",
            "{\"_index\":\"_index\",\"_type\":\"_doc\",\"_id\":\"999\",\"_version\":1,\"_score\":null,\"_source\":{\"somefield1\":\"2017-01-02T05:04:23.1\"},\"fields\":{},\"sort\":[]}",
            "{\"_index\":\"_index\",\"_type\":\"_doc\",\"_id\":\"999\",\"_version\":1,\"_score\":null,\"_source\":{\"somefield1\":\"2017-01-02T05:04:23\"},\"fields\":{},\"sort\":[]}",
            "{\"_index\":\"_index\",\"_type\":\"_doc\",\"_id\":\"999\",\"_version\":1,\"_score\":null,\"_source\":{\"somefield1\":\"2017-01-02T00:00:00\"},\"fields\":{},\"sort\":[]}", })]
        public string[] MapRowsToHits_WhenTimeZone_ReadTimezone()
        {
            var results =
                new List<Dictionary<string, object>>() {
                    new Dictionary<string, object> {
                        // 16:27 UTC
                        { "somefield1", new DateTime(637142704713640182, DateTimeKind.Utc) },
                    },
                    new Dictionary<string, object> {
                        { "somefield1", new DateTime(2017, 1, 2, 13, 45, 23, 133, DateTimeKind.Utc) },
                    },
                    new Dictionary<string, object> {
                        { "somefield1", new DateTime(2017, 1, 2, 5, 4, 23, 100, DateTimeKind.Utc) },
                    },
                    new Dictionary<string, object> {
                        { "somefield1", new DateTime(2017, 1, 2, 5, 4, 23, DateTimeKind.Utc) },
                    },
                    new Dictionary<string, object> {
                        { "somefield1", new DateTime(2017, 1, 2, 0, 0, 0, DateTimeKind.Utc) },
                    },
            };
            var response = BuildHits(results, query);
            return SetRandomProperties(response).Select(r => JsonConvert.SerializeObject(r)).ToArray();
        }

        [TestCase(ExpectedResult = JTokenType.String)]
        public JTokenType MapRowsToHits_WhenString_ReadString()
        {
            var results =
                new List<Dictionary<string, object>>() {
                    new Dictionary<string, object> {
                        { "somefield1", "somevalue1" },
                    },
            };
            var response = BuildHits(results, query);
            return response.First().Source.GetValue("somefield1", StringComparison.OrdinalIgnoreCase).Type;
        }

        [TestCase(ExpectedResult = JTokenType.Null)]
        public JTokenType MapRowsToHits_WhenDBNull_ReadNull()
        {
            var results =
                new List<Dictionary<string, object>>() {
                    new Dictionary<string, object> {
                        { "somefield1", null },
                    },
            };
            var response = BuildHits(results, query);
            return response.First().Source.GetValue("somefield1", StringComparison.OrdinalIgnoreCase).Type;
        }

        [TestCase(ExpectedResult = false)]
        public bool MapRowsToHits_WhenValidInput_ReturnsIdsAreUnique()
        {
            var results =
                    new List<Dictionary<string, object>>() {
                        new Dictionary<string, object> {
                            { "somefield1", "somevalue1" },
                        },
                        new Dictionary<string, object> {
                            { "somefield1", "somevalue2" },
                        },
                        new Dictionary<string, object> {
                            { "somefield1", "somevalue3" },
                        },
                        new Dictionary<string, object> {
                            { "somefield1", "somevalue4" },
                        },
                        new Dictionary<string, object> {
                            { "somefield1", "somevalue5" },
                        },
                        new Dictionary<string, object> {
                            { "somefield1", "somevalue6" },
                        },
                };

            var response = BuildHits(results, query);
            var hash = new Dictionary<string, int>();
            foreach (var hit in response)
            {
                if (hash.ContainsKey(hit.Id))
                {
                    return true;
                }

                hash.Add(hit.Id, 1);
            }

            return false;
        }

        private static IEnumerable<Hit> SetRandomProperties(IEnumerable<Hit> hits) => hits.Select(i =>
        {
            i.Id = HitTestId;
            return i;
        });

        /// <summary>
        /// Create a list of Hits by iterating over mock results.
        /// </summary>
        /// <param name="results">SearchAsync results as a list of rows (mapping column name to value).</param>
        /// <param name="query">Query information used to enrich hits.</param>
        /// <returns>A collection of Hits, one per item in the provided results data.</returns>
        private static IEnumerable<Hit> BuildHits(List<Dictionary<string, object>> results, QueryData query)
        {
            using var table = ToDataTable(results);
            var logger = new Mock<ILogger>();
            using var highlighter = new LuceneHighlighter(query, logger.Object);
            return HitsMapper.MapRowsToHits(table.Rows, query, highlighter);
        }

        /// <summary>
        /// Converts a list of rows to a DataTable structure.
        /// </summary>
        /// <param name="results">SearchAsync results as a list of rows (mapping column name to value).</param>
        /// <returns>A DataTable from the data in results.</returns>
        private static DataTable ToDataTable(List<Dictionary<string, object>> results)
        {
            DataTable table = new DataTable();

            foreach (var srow in results.Select((value, index) => new { value, index }))
            {
                var row = table.NewRow();
                foreach (var sval in srow.value)
                {
                    if (srow.index == 0)
                    {
                        var type = sval.Value == null ? typeof(object) : sval.Value.GetType();
                        var columnSpec = new DataColumn(sval.Key, type);
                        table.Columns.Add(columnSpec);
                    }

                    row[sval.Key] = sval.Value;
                }

                table.Rows.Add(row);
            }

            return table;
        }

        private string MakeHighlightHit(Dictionary<string, object> fields, string highlightKey, string highlightText)
        {
            var results =
                new List<Dictionary<string, object>>() {
                    fields,
                };

            var highlightQuery = new QueryData("_kql", "_index", null, new Dictionary<string, string>()
            {
                { highlightKey, highlightText },
            })
            {
                HighlightPreTag = "hl",
                HighlightPostTag = "/hl",
            };

            var response = BuildHits(results, highlightQuery);
            return JsonConvert.SerializeObject(SetRandomProperties(response).First());
        }
    }
}