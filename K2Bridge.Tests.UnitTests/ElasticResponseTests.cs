// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace Tests
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Linq;
    using K2Bridge.Models;
    using K2Bridge.Models.Response;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using NUnit.Framework;

    [TestFixture]
    public class ElasticResponseTests
    {
        private const string HitTestId = "999";

        private static readonly Random RandomId = new Random(42);

        private QueryData query = new QueryData("_kql", "_index");

        [TestCase(ExpectedResult =
            "{\"responses\":[{\"aggregations\":{\"2\":{\"buckets\":[]}},\"took\":0,\"timed_out\":false,\"_shards\":{\"total\":1,\"successful\":1,\"skipped\":0,\"failed\":0},\"hits\":{\"total\":0,\"max_score\":null,\"hits\":[]},\"status\":200}]}")]
        public string DefaultResponseHasExpectedElasticProperties()
        {
            var defaultResponse = new ElasticResponse();
            var serializedResponse = JsonConvert.SerializeObject(defaultResponse);

            return serializedResponse;
        }

        [TestCase(ExpectedResult = "{\"_index\":\"_index\",\"_type\":\"_doc\",\"_id\":\"999\",\"_version\":1,\"_score\":null,\"_source\":{},\"fields\":{},\"sort\":[]}")]
        public string ResponseWithEmptyHitHasExpectedElasticProperties()
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
        public string ResponseWithSingleHitHasHasAllFieldsInSource()
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
        public string[] ResponseWithMultipleHitHasHasAllFieldsInSource()
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
        public JTokenType TestDecimalAreReadByType()
        {
            var results =
                new List<Dictionary<string, object>>() {
                    new Dictionary<string, object> {
                        { "somefield1", 2M },
                    },
            };
            var response = BuildHits(results, query);
            return response.First().Source.GetValue("somefield1").Type;
        }

        [TestCase(ExpectedResult = JTokenType.Boolean)]
        public JTokenType TestBooleanAreReadBySByteType()
        {
            var results =
                new List<Dictionary<string, object>>() {
                    new Dictionary<string, object> {
                        { "somefield1", (sbyte)0 },
                    },
            };
            var response = BuildHits(results, query);
            return response.First().Source.GetValue("somefield1").Type;
        }

        [TestCase(ExpectedResult = JTokenType.Boolean)]
        public JTokenType TestBooleanAreReadByBooleanType()
        {
            var results =
                new List<Dictionary<string, object>>() {
                    new Dictionary<string, object> {
                        { "somefield1", false },
                    },
            };
            var response = BuildHits(results, query);
            return response.First().Source.GetValue("somefield1").Type;
        }

        [TestCase(ExpectedResult = JTokenType.Integer)]
        public JTokenType TestIntegerAreReadType()
        {
            var results =
                new List<Dictionary<string, object>>() {
                    new Dictionary<string, object> {
                        { "somefield1", 1 },
                    },
            };
            var response = BuildHits(results, query);
            return response.First().Source.GetValue("somefield1").Type;
        }

        [TestCase(ExpectedResult = JTokenType.String)]
        public JTokenType TestDateAreReadAsStringType()
        {
            var results =
                new List<Dictionary<string, object>>() {
                    new Dictionary<string, object> {
                        { "somefield1", DateTime.Now },
                    },
            };
            var response = BuildHits(results, query);
            return response.First().Source.GetValue("somefield1").Type;
        }

        [TestCase(ExpectedResult = new[] {
            "{\"_index\":\"_index\",\"_type\":\"_doc\",\"_id\":\"999\",\"_version\":1,\"_score\":null,\"_source\":{\"somefield1\":\"2020-01-10T16:27:51.3640182\"},\"fields\":{},\"sort\":[]}",
            "{\"_index\":\"_index\",\"_type\":\"_doc\",\"_id\":\"999\",\"_version\":1,\"_score\":null,\"_source\":{\"somefield1\":\"2017-01-02T13:45:23.133\"},\"fields\":{},\"sort\":[]}",
            "{\"_index\":\"_index\",\"_type\":\"_doc\",\"_id\":\"999\",\"_version\":1,\"_score\":null,\"_source\":{\"somefield1\":\"2017-01-02T05:04:23.1\"},\"fields\":{},\"sort\":[]}",
            "{\"_index\":\"_index\",\"_type\":\"_doc\",\"_id\":\"999\",\"_version\":1,\"_score\":null,\"_source\":{\"somefield1\":\"2017-01-02T05:04:23\"},\"fields\":{},\"sort\":[]}",
            "{\"_index\":\"_index\",\"_type\":\"_doc\",\"_id\":\"999\",\"_version\":1,\"_score\":null,\"_source\":{\"somefield1\":\"2017-01-02T00:00:00\"},\"fields\":{},\"sort\":[]}", })]
        public string[] TestDateTimezone()
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
        public JTokenType TestStringsAreReadType()
        {
            var results =
                new List<Dictionary<string, object>>() {
                    new Dictionary<string, object> {
                        { "somefield1", "somevalue1" },
                    },
            };
            var response = BuildHits(results, query);
            return response.First().Source.GetValue("somefield1").Type;
        }

        [TestCase(ExpectedResult = JTokenType.Null)]
        public JTokenType TestDbNullAreRead()
        {
            var results =
                new List<Dictionary<string, object>>() {
                    new Dictionary<string, object> {
                        { "somefield1", null },
                    },
            };
            var response = BuildHits(results, query);
            return response.First().Source.GetValue("somefield1").Type;
        }

        [TestCase(ExpectedResult = false)]
        public bool TestHitIdsAreUnique()
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

        private IEnumerable<Hit> SetRandomProperties(IEnumerable<Hit> hits) => hits.Select(i =>
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
        private IEnumerable<Hit> BuildHits(List<Dictionary<string, object>> results, QueryData query)
        {
            var table = ToDataTable(results);

            foreach (DataRow row in table.Rows)
            {
                var hit = Hit.Create(row, query);
                hit.Id = RandomId.Next().ToString();
                yield return hit;
            }

            table.Dispose();
        }

        /// <summary>
        /// Converts a list of rows to a DataTable structure.
        /// </summary>
        /// <param name="results">SearchAsync results as a list of rows (mapping column name to value).</param>
        /// <returns>A DataTable from the data in results.</returns>
        private DataTable ToDataTable(List<Dictionary<string, object>> results)
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
    }
}
