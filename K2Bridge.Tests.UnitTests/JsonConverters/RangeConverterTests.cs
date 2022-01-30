// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2Bridge.Tests.UnitTests.JsonConverters;

using System.Collections.Generic;
using K2Bridge.Models.Request;
using K2Bridge.Models.Request.Queries;
using NUnit.Framework;

[TestFixture]
public class RangeConverterTests
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

    private static readonly Query ExpectedValidQueryTimestampRange = new()
    {
        Bool = new BoolQuery
        {
            Must = new List<IQuery> {
                    new RangeClause()
                    {
                        FieldName = "timestamp",
                        GTEValue = "0",
                        LTEValue = "10",
                        Format = "epoch_millis",
                    },
                },
            MustNot = new List<IQuery>(),
            Should = new List<IQuery>(),
            ShouldNot = new List<IQuery>(),
            Filter = new List<IQuery> { null },
        },
    };

    private static readonly Query ExpectedValidQueryBetweenRange = new()
    {
        Bool = new BoolQuery
        {
            Must = new List<IQuery> {
                    new RangeClause()
                    {
                        FieldName = "TEST_FIELD",
                        GTEValue = "0",
                        LTValue = "10",
                    },
                },
            MustNot = new List<IQuery>(),
            Should = new List<IQuery>(),
            ShouldNot = new List<IQuery>(),
            Filter = new List<IQuery> { null },
        },
    };

    private static readonly Query ExpectedValidQueryTimestampRangeSingleNoPair = new()
    {
        Bool = new BoolQuery
        {
            Must = new List<IQuery> {
                    new RangeClause()
                    {
                        FieldName = "timestamp",
                        GTEValue = "0",
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
            new TestCaseData(QueryTimestampRangeSingle, ExpectedValidQueryTimestampRange).SetName("JsonDeserializeObject_WithQuerySimpleTimestampRange_DeserializedCorrectly"),
            new TestCaseData(QueryBetweenRangeSingle, ExpectedValidQueryBetweenRange).SetName("JsonDeserializeObject_WithQueryFieldBetweenRange_DeserializedCorrectly"),
            new TestCaseData(QueryTimestampRangeSingleNoPair, ExpectedValidQueryTimestampRangeSingleNoPair).SetName("JsonDeserializeObject_WithQueryTimestampRangeNoPair_DeserializedCorrectly"),
        };

    [TestCaseSource(nameof(RangeTestCases))]
    public void TestRangeQueryStringQueries(string queryString, object expected)
    {
        queryString.AssertJsonString((Query)expected);
    }
}
