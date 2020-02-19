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
        private const string MatchWithoutQueryProperty = @"
            {""bool"":
                {""must"":
                    [
                        {""match_phrase"":
                            {
                                ""field_name"" : ""this is a test""
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

        private const string MatchWithQueryProperty = @"
            {""bool"":
                {""must"":
                    [
                        {""match_phrase"":
                            {
                                ""field_name"" :
                                    {
                                    ""query"" : ""this is a test""
                                    }
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

        private static readonly Query ExpectedValidMatchClause = new Query
        {
            Bool = new BoolQuery
            {
                Must = new List<IQuery> {
                    new MatchPhraseClause
                    {
                        FieldName = "field_name",
                        Phrase = "this is a test",
                    },
               },
                MustNot = new List<IQuery>(),
                Should = new List<IQuery>(),
                ShouldNot = new List<IQuery>(),
                Filter = new List<IQuery> { null },
            },
        };

        private static readonly object[] MatchTestCases = {
            new TestCaseData(MatchWithoutQueryProperty, ExpectedValidMatchClause).SetName("ValidMatchClause_WithoutQueryProperty_DeserializedCorrectly"),
            new TestCaseData(MatchWithQueryProperty, ExpectedValidMatchClause).SetName("ValidMatchClause_WithQueryProperty_DeserializedCorrectly"),
        };

        [TestCaseSource(nameof(MatchTestCases))]
        public void TestMatchClauseQueries(string queryString, object expected)
        {
            queryString.AssertJsonString((Query)expected);
        }
    }
}