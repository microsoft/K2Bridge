// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2Bridge.Tests.UnitTests.JsonConverters;

using System.Collections.Generic;
using K2Bridge.Models.Request;
using K2Bridge.Models.Request.Queries;
using NUnit.Framework;

[TestFixture]
public partial class JsonConvertersTests
{
    private const string RangeClause = @"
                        {""range"":
                            {""timestamp"":
                                {""gte"":0,""lte"":10,""format"":""epoch_millis""}
                            }
                        }";

    private const string MatchClause = @"
                        {""match_phrase"":
                            {
                                ""field_name"" : ""this is a test""
                            }
                        }";

    private const string ExistsClause = @"
                        {""exists"": {
                            ""field"": ""field_name""
                          }
                        }";

    private const string QueryStringClause = @"
                        {""query_string"": {
                            ""query"": ""field_name: test"",
                            ""analyze_wildcard"": true,
                            ""default_field"": ""*""
                          }
                        }";

    private const string BoolQuery = @"
            {""bool"":
                {""must"":[],
                    ""filter"":
                    [
                        {""match_all"":{}}
                    ],
                    ""should"":[],
                    ""must_not"":[]
                }
            }";

    private const string DoNotExistLeafClause = @"{""non_existing"": ""leaf_clause""}";

    private static readonly ILeafClause ExpectedRangeClause =
                new RangeClause
                {
                    FieldName = "timestamp",
                    GTEValue = "0",
                    LTEValue = "10",
                    Format = "epoch_millis",
                };

    private static readonly ILeafClause ExpectedMatchClause =
                new MatchPhraseClause
                {
                    FieldName = "field_name",
                    Phrase = "this is a test",
                };

    private static readonly ILeafClause ExpectedExistsClause =
                new ExistsClause
                {
                    FieldName = "field_name",
                };

    private static readonly ILeafClause ExpectedQueryStringClause =
                new QueryStringClause
                {
                    Phrase = "field_name: test",
                    Wildcard = true,
                    Default = "*",
                };

    private static readonly IQuery ExpectedBoolQuery = new BoolQuery
    {
        Must = new List<IQuery>(),
        MustNot = new List<IQuery>(),
        Should = new List<IQuery>(),
        ShouldNot = new List<IQuery>(),
        Filter = new List<IQuery> { null },
    };

    private static readonly object[] LeafClauseTestCases = {
            new TestCaseData(RangeClause, ExpectedRangeClause).SetName("LeafRangeClause_WithSimpleRange_DeserializedCorrectly"),
            new TestCaseData(MatchClause, ExpectedMatchClause).SetName("LeafMatchClause_WithSimplePhrase_DeserializedCorrectly"),
            new TestCaseData(ExistsClause, ExpectedExistsClause).SetName("LeafExistsClause_WithExistsFilter_DeserializedCorrectly"),
            new TestCaseData(QueryStringClause, ExpectedQueryStringClause).SetName("LeafQueryStringClause_WithSimplePhrase_DeserializedCorrectly"),
            new TestCaseData(DoNotExistLeafClause, null).SetName("LeafClause_NonExisting_ConvertedToNull"),
        };

    private static readonly object[] LeafClauseQueryTestCases = {
            new TestCaseData(BoolQuery, ExpectedBoolQuery).SetName("BoolQuery_All_DeserializedCorrectly"),
        };

    [TestCaseSource(nameof(LeafClauseTestCases))]
    public void TestLeafClauseConversions(string queryString, object expected)
    {
        queryString.AssertJsonString((ILeafClause)expected);
    }

    [TestCaseSource(nameof(LeafClauseQueryTestCases))]
    public void TestQueryConversions(string queryString, object expected)
    {
        queryString.AssertJsonString((IQuery)expected);
    }
}
