// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace UnitTests.K2Bridge.Visitors
{
    using global::K2Bridge.Models.Request.Queries;
    using global::K2Bridge.Tests.UnitTests.Visitors;
    using global::K2Bridge.Visitors;
    using Newtonsoft.Json;
    using NUnit.Framework;

    [TestFixture]
    public class ParseElasticToKqlTests
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
            ExpectedResult = "where (['TEST_FIELD'] == \"TEST_RESULT\")",
            TestName = "QueryAccept_WithSingleMatchPhrase_ReturnsExpectedResult")]
        [TestCase(
            QueryMatchPhraseMulti,
            ExpectedResult = "where (['TEST_FIELD'] == \"TEST_RESULT\") and (['TEST_FIELD_2'] == \"TEST_RESULT_2\")",
            TestName = "QueryAccept_WithMultiMatchPhrase_ReturnsExpectedResult")]
        public string TestMatchPhraseQueries(string queryString)
        {
            var query = JsonConvert.DeserializeObject<Query>(queryString);
            var visitor = VisitorTestsUtils.CreateAndVisitRootVisitor();
            query.Accept(visitor);
            return query.KustoQL;
        }

        [TestCase(
            QueryExists,
            ExpectedResult = "where (isnotnull(['TEST_FIELD']))",
            TestName = "QueryAccept_WithSingleExists_ReturnsExpectedResult")]
        public string TestExistsClause(string queryString)
        {
            var query = JsonConvert.DeserializeObject<Query>(queryString);
            var visitor = VisitorTestsUtils.CreateAndVisitRootVisitor();
            query.Accept(visitor);
            return query.KustoQL;
        }

        [TestCase(
            QueryTimestampRangeSingle,
            ExpectedResult = "where (['timestamp'] >= unixtime_milliseconds_todatetime(0) and ['timestamp'] <= unixtime_milliseconds_todatetime(10))",
            TestName = "QueryAccept_WithTimestamp_ReturnsExpectedResult")]
        [TestCase(
            QueryBetweenRangeSingle,
            ExpectedResult = "where (['TEST_FIELD'] >= 0 and ['TEST_FIELD'] < 10)",
            TestName = "QueryAccept_WithBetweenRange_ReturnsExpectedResult")]
        public string TestRangeQueries(string queryString)
        {
            return TestRangeClause(queryString, "TEST_FIELD", "long");
        }

        [TestCase(
            QueryTimestampRangeSingleNoPair,
            TestName = "QueryAccept_WithBetweenRangeSingleNoPair_ReturnsExpectedResult")]
        public void TestRangeQueriesMissingValues(string queryString)
        {
            Assert.Throws(typeof(IllegalClauseException), () => TestRangeClause(queryString));
        }

        [TestCase(
            QueryString,
            ExpectedResult = "where (* has \"TEST_RESULT\")",
            TestName = "QueryAccept_WithValidInput_ReturnsExpectedResult")]
        public string TestQueryStringQueries(string queryString)
        {
            var query = JsonConvert.DeserializeObject<Query>(queryString);
            var visitor = VisitorTestsUtils.CreateAndVisitRootVisitor();
            query.Accept(visitor);
            return query.KustoQL;
        }

        [TestCase(
            CombinedQuery,
            ExpectedResult = "where (* has \"TEST_RESULT\") and (['TEST_FIELD'] == \"TEST_RESULT_2\") and (['TEST_FIELD_2'] == \"TEST_RESULT_3\") and (['timestamp'] >= unixtime_milliseconds_todatetime(0) and ['timestamp'] <= unixtime_milliseconds_todatetime(10))",
            TestName = "QueryAccept_WithCombinedQuery_ReturnsExpectedResult")]
        [TestCase(
            NotQueryStringClause,
            ExpectedResult = "where (* has \"TEST_RESULT\") and (['TEST_FIELD'] != \"TEST_RESULT_2\")",
            TestName = "QueryAccept_WithNotString_ReturnsExpectedResult")]
        public string TestCombinedQueries(string queryString)
        {
            var query = JsonConvert.DeserializeObject<Query>(queryString);
            var visitor = VisitorTestsUtils.CreateAndVisitRootVisitor();
            query.Accept(visitor);
            return query.KustoQL;
        }

        [TestCase(
            QueryWildcardString,
            ExpectedResult = "where (* matches regex \"TEST(.)*RESULT\")",
            TestName = "QueryAccept_WithWildCard_ReturnsExpectedResult")]
        public string TestWildcardQuery(string queryString)
        {
            var query = JsonConvert.DeserializeObject<Query>(queryString);
            var visitor = VisitorTestsUtils.CreateAndVisitRootVisitor();
            query.Accept(visitor);
            return query.KustoQL;
        }

        [TestCase(
            QueryComplexWildcardString,
            ExpectedResult = "where (* matches regex \"TEST(.)*RESULT(.)*\")",
            TestName = "QueryAccept_WithComplexWildCard_ReturnsExpectedResult")]
        public string TestComplexWildcardQuery(string queryString)
        {
            var query = JsonConvert.DeserializeObject<Query>(queryString);
            var visitor = VisitorTestsUtils.CreateAndVisitRootVisitor();
            query.Accept(visitor);
            return query.KustoQL;
        }

        [TestCase(
            QueryPrefixString,
            ExpectedResult = "where (* hasprefix \"TEST_RESULT\")",
            TestName = "QueryAccept_WithPrefix_ReturnsExpectedResult")]
        public string TestPrefixQuery(string queryString)
        {
            var query = JsonConvert.DeserializeObject<Query>(queryString);
            var visitor = VisitorTestsUtils.CreateAndVisitRootVisitor();
            query.Accept(visitor);
            return query.KustoQL;
        }

        private string TestRangeClause(string queryString, string field = "MyField", string type = "string")
        {
            var query = JsonConvert.DeserializeObject<Query>(queryString);
            var visitor = VisitorTestsUtils.CreateAndVisitRootVisitor(field, type);
            query.Accept(visitor);
            return query.KustoQL;
        }
    }
}
