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

        const string queryRangeSingle =  @"
            {""bool"":
                {""must"":
                    [
                        {""range"":
                            {""TEST_FIELD"":
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
            

        [TestCase(queryMatchPhraseSingle, ExpectedResult ="where (TEST_FIELD == \"TEST_RESULT\")")]
        [TestCase(queryMatchPhraseMulti, ExpectedResult ="where (TEST_FIELD == \"TEST_RESULT\") and (TEST_FIELD_2 == \"TEST_RESULT_2\")")]
        public string TestMatchPhraseQueries(string queryString)
        {
            var query = JsonConvert.DeserializeObject<Query>(queryString);
            var visitor = new ElasticSearchDSLVisitor();
            query.Accept(visitor);
            return query.KQL;
        }
        
        [TestCase(queryRangeSingle, ExpectedResult ="where (TEST_FIELD between (fromUnixTimeMilli(0)..fromUnixTimeMilli(10)))")]
        public string TestRangeQueries(string queryString)
        {
            var query = JsonConvert.DeserializeObject<Query>(queryString);
            var visitor = new ElasticSearchDSLVisitor();
            query.Accept(visitor);
            return query.KQL;
        }
    }
}