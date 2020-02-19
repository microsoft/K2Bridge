// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace UnitTests.K2Bridge.JsonConverters
{
    using System.Collections.Generic;
    using global::K2Bridge.Models.Request;
    using global::K2Bridge.Models.Request.Queries;
    using NUnit.Framework;

    [TestFixture]
    public partial class JsonConvertersTests
    {
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

        private static readonly Query ExpectedValidQueryTimestampRange = new Query
        {
            Bool = new BoolQuery
            {
                Must = new List<IQuery> {
                    new RangeClause()
                    {
                        FieldName = "timestamp",
                        GTEValue = 0,
                        LTEValue = 10,
                        Format = "epoch_millis",
                    },
                },
                MustNot = new List<IQuery>(),
                Should = new List<IQuery>(),
                ShouldNot = new List<IQuery>(),
                Filter = new List<IQuery> { null },
            },
        };

        private static readonly Query ExpectedValidQueryBetweenRange = new Query
        {
            Bool = new BoolQuery
            {
                Must = new List<IQuery> {
                    new RangeClause()
                    {
                        FieldName = "TEST_FIELD",
                        GTEValue = 0,
                        LTValue = 10,
                    },
                },
                MustNot = new List<IQuery>(),
                Should = new List<IQuery>(),
                ShouldNot = new List<IQuery>(),
                Filter = new List<IQuery> { null },
            },
        };

        private static readonly Query ExpectedValidQueryTimestampRangeSingleNoPair = new Query
        {
            Bool = new BoolQuery
            {
                Must = new List<IQuery> {
                    new RangeClause()
                    {
                        FieldName = "timestamp",
                        GTEValue = 0,
                        Format = "epoch_millis",
                    },
                },
                MustNot = new List<IQuery>(),
                Should = new List<IQuery>(),
                ShouldNot = new List<IQuery>(),
                Filter = new List<IQuery> { null },
            },
        };

        private static readonly object[] RangeTestCases = {
            new TestCaseData(QueryTimestampRangeSingle, ExpectedValidQueryTimestampRange).SetName("ValidRangeClause_SimpleTimestampRange_DeserializedCorrectly"),
            new TestCaseData(QueryBetweenRangeSingle, ExpectedValidQueryBetweenRange).SetName("ValidRangeClause_FieldBetweenRange_DeserializedCorrectly"),
            new TestCaseData(QueryTimestampRangeSingleNoPair, ExpectedValidQueryTimestampRangeSingleNoPair).SetName("ValidRangeClause_TimestampRangeNoPair_DeserializedCorrectly"),
        };

        [TestCaseSource(nameof(RangeTestCases))]
        public void TestRangeQueryStringQueries<T>(string queryString, T expected)
        {
            TestQueryStringQueriesInternal(queryString, expected);
        }
    }
}