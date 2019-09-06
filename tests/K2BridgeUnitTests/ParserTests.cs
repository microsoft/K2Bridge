using System.Collections;
using K2Bridge;
using Newtonsoft.Json;
using NUnit.Framework;

namespace Tests
{
    [TestFixture]
    public class TestParseElasticToKql
    {
        
        const string queryMatchPhraseSingle =  @"
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
        
        const string queryMatchPhraseMulti =  @"
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

        const string queryTimestampRangeSingle =  @"
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

        const string queryBetweenRangeSingle = @"
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

        const string queryStringQuery = @"
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

        const string combinedQuery = @"
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

        const string onlyNotQuery = @"
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

        [TestCase(queryMatchPhraseSingle, ExpectedResult ="where (TEST_FIELD == \"TEST_RESULT\")")]
        [TestCase(queryMatchPhraseMulti, ExpectedResult ="where (TEST_FIELD == \"TEST_RESULT\") and (TEST_FIELD_2 == \"TEST_RESULT_2\")")]
        public string TestMatchPhraseQueries(string queryString)
        {
            var query = JsonConvert.DeserializeObject<Query>(queryString);
            var visitor = new ElasticSearchDSLVisitor();
            query.Accept(visitor);
            return query.KQL;
        }
        
        [TestCase(queryTimestampRangeSingle, ExpectedResult ="where (timestamp between (fromUnixTimeMilli(0) .. fromUnixTimeMilli(10)))")]
        [TestCase(queryBetweenRangeSingle, ExpectedResult = "where (TEST_FIELD between (0 .. 10))")]
        public string TestRangeQueries(string queryString)
        {
            var query = JsonConvert.DeserializeObject<Query>(queryString);
            var visitor = new ElasticSearchDSLVisitor();
            query.Accept(visitor);
            return query.KQL;
        }

        [TestCase(queryStringQuery, ExpectedResult = "search TEST_RESULT | project-away $table")]
        public string TestQueryStringQueries(string queryString)
        {
            var query = JsonConvert.DeserializeObject<Query>(queryString);
            var visitor = new ElasticSearchDSLVisitor();
            query.Accept(visitor);
            return query.KQL;
        }

        [TestCase(combinedQuery, ExpectedResult = "search TEST_RESULT | project-away $table\n| where (TEST_FIELD == \"TEST_RESULT_2\") and (TEST_FIELD_2 == \"TEST_RESULT_3\") and (timestamp between (fromUnixTimeMilli(0) .. fromUnixTimeMilli(10)))")]
        public string TestCombinedQueries(string queryString)
        {
            var query = JsonConvert.DeserializeObject<Query>(queryString);
            var visitor = new ElasticSearchDSLVisitor();
            query.Accept(visitor);
            return query.KQL;
        }

        [TestCase(onlyNotQuery, ExpectedResult = "search TEST_RESULT | project-away $table\n| where not (TEST_FIELD == \"TEST_RESULT_2\")")]
        public string TestNotQueries(string queryString)
        {
            var query = JsonConvert.DeserializeObject<Query>(queryString);
            var visitor = new ElasticSearchDSLVisitor();
            query.Accept(visitor);
            return query.KQL;
        }
    }
}