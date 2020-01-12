// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace Tests
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Linq;
    using K2Bridge;
    using K2Bridge.Models.Response;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using NUnit.Framework;

    [TestFixture]
    public class ElasticResponseTests
    {
        private const string HIT_TEST_ID = "999";

        private static readonly Random RandomId = new Random(42);

        private QueryData query = new QueryData("_kql", "_index", null);

        [TestCase(ExpectedResult = "{\"responses\":[{\"aggregations\":{\"2\":{\"buckets\":[]}},\"took\":0,\"timed_out\":false,\"_shards\":{\"total\":1,\"successful\":1,\"skipped\":0,\"failed\":0},\"hits\":{\"total\":0,\"max_score\":null,\"hits\":[]},\"status\":200,\"backendQuery\":\"\"}]}")]
        public string DefaultResponseHasExpectedElasticProperties()
        {
            var defaultResponse = new ElasticResponse();
            var serializedResponse = JsonConvert.SerializeObject(defaultResponse);

            return serializedResponse;
        }

        [TestCase(ExpectedResult = "{\"_index\":\"_index\",\"_type\":\"_doc\",\"_id\":\"999\",\"_version\":1,\"_score\":null,\"_source\":{},\"highlight\":{}}")]
        public string ResponseWithEmptyHitHasExpectedElasticProperties()
        {
            var results = (
                new List<Dictionary<string, object>>() {
                    new Dictionary<string, object> {
                    },
                });
            var response = BuildHits(results, query);

            return JsonConvert.SerializeObject(SetRandomProperties(response).First());
        }

        [TestCase(ExpectedResult = "{\"_index\":\"_index\",\"_type\":\"_doc\",\"_id\":\"999\",\"_version\":1,\"_score\":null,\"_source\":{\"somefield1\":\"somevalue1\",\"somefield2\":\"somevalue2\",\"somefield3\":\"somevalue3\"},\"highlight\":{}}")]
        public string ResponseWithSingleHitHasHasAllFieldsInSource()
        {
            var results = (
                new List<Dictionary<string, object>>() {
                    new Dictionary<string, object> {
                        { "somefield1", "somevalue1" },
                        { "somefield2", "somevalue2" },
                        { "somefield3", "somevalue3" },
                    },
                });
            var response = BuildHits(results, query);
            return JsonConvert.SerializeObject(SetRandomProperties(response).First());
        }

        [TestCase(ExpectedResult = new[] {
            "{\"_index\":\"_index\",\"_type\":\"_doc\",\"_id\":\"999\",\"_version\":1,\"_score\":null,\"_source\":{\"somefield1\":\"somevalue1\",\"somefield2\":\"somevalue2\",\"somefield3\":\"somevalue3\"},\"highlight\":{}}",
            "{\"_index\":\"_index\",\"_type\":\"_doc\",\"_id\":\"999\",\"_version\":1,\"_score\":null,\"_source\":{\"somefield1\":\"somevalue4\",\"somefield2\":\"somevalue5\",\"somefield3\":\"somevalue6\"},\"highlight\":{}}", })]
        public string[] ResponseWithMultipleHitHasHasAllFieldsInSource()
        {
            var results = (
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
            });
            var response = BuildHits(results, query);

            return SetRandomProperties(response).Select(r => JsonConvert.SerializeObject(r)).ToArray();
        }


        [TestCase(ExpectedResult = JTokenType.Float)]
        public JTokenType TestDecimalAreReadByType()
        {
            var results = (
                new List<Dictionary<string, object>>() {
                    new Dictionary<string, object> {
                        { "somefield1", (decimal)2 },
                    },
            });
            var response = BuildHits(results, query);
            return response.First().Source.GetValue("somefield1").Type;
        }

        [TestCase(ExpectedResult = JTokenType.Boolean)]
        public JTokenType TestBooleanAreReadBySByteType()
        {
            var results = (
                new List<Dictionary<string, object>>() {
                    new Dictionary<string, object> {
                        { "somefield1", (sbyte)0 },
                    },
            });
            var response = BuildHits(results, query);
            return response.First().Source.GetValue("somefield1").Type;
        }

        [TestCase(ExpectedResult = JTokenType.Boolean)]
        public JTokenType TestBooleanAreReadByBooleanType()
        {
            var results = (
                new List<Dictionary<string, object>>() {
                    new Dictionary<string, object> {
                        { "somefield1", false },
                    },
            });
            var response = BuildHits(results, query);
            return response.First().Source.GetValue("somefield1").Type;
        }

        [TestCase(ExpectedResult = JTokenType.Integer)]
        public JTokenType TestIntegerAreReadType()
        {
            var results = (
                new List<Dictionary<string, object>>() {
                    new Dictionary<string, object> {
                        { "somefield1", 1 },
                    },
            });
            var response = BuildHits(results, query);
            return response.First().Source.GetValue("somefield1").Type;
        }

        [TestCase(ExpectedResult = JTokenType.Date)]
        public JTokenType TestDateAreReadType()
        {
            var results = (
                new List<Dictionary<string, object>>() {
                    new Dictionary<string, object> {
                        { "somefield1", DateTime.Now },
                    },
            });
            var response = BuildHits(results, query);
            return response.First().Source.GetValue("somefield1").Type;
        }

        [TestCase(ExpectedResult = JTokenType.String)]
        public JTokenType TestStringsAreReadType()
        {
            var results = (
                new List<Dictionary<string, object>>() {
                    new Dictionary<string, object> {
                        { "somefield1", "somevalue1" },
                    },
            });
            var response = BuildHits(results, query);
            return response.First().Source.GetValue("somefield1").Type;
        }

        [TestCase(ExpectedResult = JTokenType.Null)]
        public JTokenType TestDbNullAreRead()
        {
            var results = (
                new List<Dictionary<string, object>>() {
                    new Dictionary<string, object> {
                        { "somefield1", null },
                    },
            });
            var response = BuildHits(results, query);
            return response.First().Source.GetValue("somefield1").Type;
        }

        [TestCase(ExpectedResult = false)]
        public bool TestHitIdsAreUnique()
        {
            var results = (
                    new List<Dictionary<string, object>>() {
                        new Dictionary<string, object> {
                            { "somefield1", "somevalue1" },
                        },
                        new Dictionary<string, object> {
                            { "somefield1", "somevalue2" },
                        } ,
                        new Dictionary<string, object> {
                            { "somefield1", "somevalue3" },
                        },
                        new Dictionary<string, object> {
                            { "somefield1", "somevalue4" },
                        } ,
                        new Dictionary<string, object> {
                            { "somefield1", "somevalue5" },
                        },
                        new Dictionary<string, object> {
                            { "somefield1", "somevalue6" },
                        },
                });

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
            i.Id = HIT_TEST_ID;
            return i;
        });

        /// <summary>
        /// Create a list of Hits by iterating over mock results.
        /// </summary>
        /// <param name="results">Search results as a list of rows (mapping column name to value).</param>
        /// <param name="query">Query information used to enrich hits.</param>
        /// <returns>A collection of Hits, one per item in the provided results data</returns>
        private IEnumerable<Hit> BuildHits(List<Dictionary<string, object>> results, QueryData query)
        {
            var table = ToDataTable(results);

            foreach (DataRow row in table.Rows)
            {
                var hit = Hit.Create(row, query);
                hit.Id = RandomId.Next().ToString();
                yield return hit;
            }
        }

        /// <summary>
        /// Converts a list of rows to a DataTable structure.
        /// </summary>
        /// <param name="results">Search results as a list of rows (mapping column name to value).</param>
        /// <returns>A DataTable from the data in results</returns>
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