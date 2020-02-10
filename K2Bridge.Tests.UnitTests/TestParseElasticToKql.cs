// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace Tests
{
    using K2Bridge.Models.Request.Queries;
    using K2Bridge.Visitors;
    using Newtonsoft.Json;
    using NUnit.Framework;

    [TestFixture]
    public class TestParseElasticToKql
    {
        private const string QueryExists = @"
            {""bool"":
                {""must"":
                    [
                        {""exists"": {
                            ""field"": ""TEST_FIELD""}
                        }
                    ],
                    ""filter"":
                    [
                        {""match_all"":{}}
                    ],
                    ""should"":[],
                    ""must_not"":[]
                }
            }";

        private const string QueryMatchPhraseSingle = @"
            {""bool"":
                {""must"":
                    [
                        {""match_phrase"":
                            {""TEST_FIELD"":
                                {""query"":""TEST_RESULT""}
                            }
                        }
                    ],
                    ""filter"":
                    [
                        {""match_all"":{}}
                    ],
                    ""should"":[],
                    ""must_not"":[]
                }
            }";

        private const string QueryMatchPhraseMulti = @"
            {""bool"":
                {""must"":
                    [
                        {""match_phrase"":
                            {""TEST_FIELD"":
                                {""query"":""TEST_RESULT""}
                            }
                        },
                        {""match_phrase"":
                            {""TEST_FIELD_2"":
                                {""query"":""TEST_RESULT_2""}
                            }
                        }
                    ],
                    ""filter"":
                    [
                        {""match_all"":{}}
                    ],
                    ""should"":[],
                    ""must_not"":[]
                }
            }";

        private const string QueryTimestampRangeSingle = @"
            {""bool"":
                {""must"":
                    [
                        {""range"":
                            {""timestamp"":
                                {""gte"":0,""lte"":10,""format"":""epoch_millis""}
                            }
                        }
                    ],
                    ""filter"":
                    [
                        {""match_all"":{}}
                    ],
                    ""should"":[],
                    ""must_not"":[]
                }
            }";

        private const string QueryBetweenRangeSingle = @"
            {""bool"":
                {""must"":
                    [
                        {""range"":
                            {""TEST_FIELD"":
                                {""gte"":0,""lt"":10}
                            }
                        }
                    ],
                    ""filter"":
                    [
                        {""match_all"":{}}
                    ],
                    ""should"":[],
                    ""must_not"":[]
                }
            }";

        private const string QueryTimestampRangeSingleNoPair = @"
            {""bool"":
                {""must"":
                    [
                        {""range"":
                            {""timestamp"":
                                {""gte"":0,""format"":""epoch_millis""}
                            }
                        }
                    ],
                    ""filter"":
                    [
                        {""match_all"":{}}
                    ],
                    ""should"":[],
                    ""must_not"":[]
                }
            }";

        private const string QueryString = @"
            {""bool"":
                {""must"":
                    [
                        {""query_string"":
             	            {""query"": ""TEST_RESULT"",
                             ""analyze_wildcard"": true,
                             ""default_field"": ""*""
                            }
                        }
                    ],
                    ""filter"":
                    [
                        {""match_all"":{}}
                    ],
                    ""should"":[],
                    ""must_not"":[]
                }
            }";

        private const string QueryPrefixString = @"
            {""bool"":
                {""must"":
                    [
                        {""query_string"":
             	            {""query"": ""TEST_RESULT*"",
                             ""analyze_wildcard"": true,
                             ""default_field"": ""*""
                            }
                        }
                    ],
                    ""filter"":
                    [
                        {""match_all"":{}}
                    ],
                    ""should"":[],
                    ""must_not"":[]
                }
            }";

        private const string QueryComplexWildcardString = @"
            {""bool"":
                {""must"":
                    [
                        {""query_string"":
             	            {""query"": ""TEST*RESULT*"",
                             ""analyze_wildcard"": true,
                             ""default_field"": ""*""
                            }
                        }
                    ],
                    ""filter"":
                    [
                        {""match_all"":{}}
                    ],
                    ""should"":[],
                    ""must_not"":[]
                }
            }";

        private const string QueryWildcardString = @"
            {""bool"":
                {""must"":
                    [
                        {""query_string"":
             	            {""query"": ""TEST*RESULT"",
                             ""analyze_wildcard"": true,
                             ""default_field"": ""*""
                            }
                        }
                    ],
                    ""filter"":
                    [
                        {""match_all"":{}}
                    ],
                    ""should"":[],
                    ""must_not"":[]
                }
            }";

        private const string CombinedQuery = @"
            {""bool"":
                {""must"":
                    [
                        {""query_string"":
             	            {""query"": ""TEST_RESULT"",
                             ""analyze_wildcard"": true,
                             ""default_field"": ""*""
                            }
                        },
                        {""match_phrase"":
                            {""TEST_FIELD"":
                                {""query"":""TEST_RESULT_2""}
                            }
                        },
                        {""match_phrase"":
                            {""TEST_FIELD_2"":
                                {""query"":""TEST_RESULT_3""}
                            }
                        },
                        {""range"":
                            {""timestamp"":
                                {""gte"":0,""lte"":10,""format"":""epoch_millis""}
                            }
                        }
                    ],
                    ""filter"":
                    [
                        {""match_all"":{}}
                    ],
                    ""should"":[],
                    ""must_not"":[]
                }
            }";

        private const string NotQueryStringClause = @"
            {""bool"":
                {""must"":
                    [
                        {""query_string"":
             	            {""query"": ""TEST_RESULT"",
                             ""analyze_wildcard"": true,
                             ""default_field"": ""*""
                            }
                        },
                    ],
                    ""filter"":
                    [
                        {""match_all"":{}}
                    ],
                    ""should"":[],
                    ""must_not"":
                     [
                        {""match_phrase"":
                            {""TEST_FIELD"":
                                {""query"":""TEST_RESULT_2""}
                            }
                        }
                    ]
                }
            }";

        [TestCase(
            QueryMatchPhraseSingle,
            ExpectedResult = "where (TEST_FIELD == \"TEST_RESULT\")")]
        [TestCase(
            QueryMatchPhraseMulti,
            ExpectedResult = "where (TEST_FIELD == \"TEST_RESULT\") and (TEST_FIELD_2 == \"TEST_RESULT_2\")")]
        public string TestMatchPhraseQueries(string queryString)
        {
            var query = JsonConvert.DeserializeObject<Query>(queryString);
            var visitor = new ElasticSearchDSLVisitor(LazySchemaRetrieverMock.CreateMockSchemaRetriever());
            query.Accept(visitor);
            return query.KustoQL;
        }

        [TestCase(
            QueryExists,
            ExpectedResult = "where (isnotnull(TEST_FIELD))")]
        public string TestExistsClause(string queryString)
        {
            var query = JsonConvert.DeserializeObject<Query>(queryString);
            var visitor = new ElasticSearchDSLVisitor(LazySchemaRetrieverMock.CreateMockSchemaRetriever());
            query.Accept(visitor);
            return query.KustoQL;
        }

        [TestCase(
            QueryTimestampRangeSingle,
            ExpectedResult = "where (timestamp >= fromUnixTimeMilli(0) and timestamp <= fromUnixTimeMilli(10))")]
        [TestCase(
            QueryBetweenRangeSingle,
            ExpectedResult = "where (TEST_FIELD >= 0 and TEST_FIELD < 10)")]
        public string TestRangeQueries(string queryString)
        {
            return TestRangeClause(queryString);
        }

        [TestCase(QueryTimestampRangeSingleNoPair)]
        public void TestRangeQueriesMissingValues(string queryString)
        {
            Assert.Throws(typeof(IllegalClauseException), () => TestRangeClause(queryString));
        }

        [TestCase(QueryString, ExpectedResult = "where (* has \"TEST_RESULT\")")]
        public string TestQueryStringQueries(string queryString)
        {
            var query = JsonConvert.DeserializeObject<Query>(queryString);
            var visitor = new ElasticSearchDSLVisitor(LazySchemaRetrieverMock.CreateMockSchemaRetriever());
            query.Accept(visitor);
            return query.KustoQL;
        }

        [TestCase(CombinedQuery, ExpectedResult = "where (* has \"TEST_RESULT\") and (TEST_FIELD == \"TEST_RESULT_2\") and (TEST_FIELD_2 == \"TEST_RESULT_3\") and (timestamp >= fromUnixTimeMilli(0) and timestamp <= fromUnixTimeMilli(10))")]
        [TestCase(NotQueryStringClause, ExpectedResult = "where (* has \"TEST_RESULT\") and not (TEST_FIELD == \"TEST_RESULT_2\")")]
        public string TestCombinedQueries(string queryString)
        {
            var query = JsonConvert.DeserializeObject<Query>(queryString);
            var visitor = new ElasticSearchDSLVisitor(LazySchemaRetrieverMock.CreateMockSchemaRetriever());
            query.Accept(visitor);
            return query.KustoQL;
        }

        [TestCase(
            QueryWildcardString,
            ExpectedResult = "where (* matches regex \"TEST[.\\\\S]*RESULT\")")]
        public string TestWildcardQuery(string queryString)
        {
            var query = JsonConvert.DeserializeObject<Query>(queryString);
            var visitor = new ElasticSearchDSLVisitor(LazySchemaRetrieverMock.CreateMockSchemaRetriever());
            query.Accept(visitor);
            return query.KustoQL;
        }

        [TestCase(
            QueryComplexWildcardString,
            ExpectedResult = "where (* matches regex \"TEST[.\\\\S]*RESULT[.\\\\S]*\")")]
        public string TestComplexWildcardQuery(string queryString)
        {
            var query = JsonConvert.DeserializeObject<Query>(queryString);
            var visitor = new ElasticSearchDSLVisitor(LazySchemaRetrieverMock.CreateMockSchemaRetriever());
            query.Accept(visitor);
            return query.KustoQL;
        }

        [TestCase(
            QueryPrefixString,
            ExpectedResult = "where (* hasprefix_cs \"TEST_RESULT\")")]
        public string TestPrefixQuery(string queryString)
        {
            var query = JsonConvert.DeserializeObject<Query>(queryString);
            var visitor = new ElasticSearchDSLVisitor(LazySchemaRetrieverMock.CreateMockSchemaRetriever());
            query.Accept(visitor);
            return query.KustoQL;
        }

        private string TestRangeClause(string queryString)
        {
            var query = JsonConvert.DeserializeObject<Query>(queryString);
            var visitor = new ElasticSearchDSLVisitor(LazySchemaRetrieverMock.CreateMockSchemaRetriever());
            query.Accept(visitor);
            return query.KustoQL;
        }
    }
}