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
    public class QueryStringClauseConverterTests
    {
        private const string ValidQuery = @"
            {""bool"":
                {""must"":
                    [
                        {
                           ""query_string"": {
                           ""query"": ""TEST_FIELD:[0 TO 2]"",
                           ""analyze_wildcard"": true,
                           ""default_field"": ""-""
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

        private const string QueryMissingAnalyzeWildcardProperty = @"
            {""bool"":
                {""must"":
                    [
                        {
                           ""query_string"": {
                           ""query"": ""TEST_FIELD:[0 TO 2]"",
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

        private const string QueryMissingDefaultFieldProperty = @"
            {""bool"":
                {""must"":
                    [
                        {
                           ""query_string"": {
                           ""query"": ""TEST_FIELD:[0 TO 2]"",
                           ""analyze_wildcard"": true,
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

        private static readonly Query ExpectedValidQuery = new Query
        {
            Bool = new BoolQuery
            {
                Must = new List<IQuery> {
                    new QueryStringClause()
                    {
                        Phrase = "TEST_FIELD:[0 TO 2]",
                        Wildcard = true,
                        Default = "-",
                    },
                },
                MustNot = new List<IQuery>(),
                Should = new List<IQuery>(),
                ShouldNot = new List<IQuery>(),
                Filter = new List<IQuery> { null },
            },
        };

        private static readonly Query ExpectedValidQueryMissingAnalyzeWildcard = new Query
        {
            Bool = new BoolQuery
            {
                Must = new List<IQuery> {
                    new QueryStringClause()
                    {
                        Phrase = "TEST_FIELD:[0 TO 2]",
                        Wildcard = false,
                        Default = "*",
                    },
                },
                MustNot = new List<IQuery>(),
                Should = new List<IQuery>(),
                ShouldNot = new List<IQuery>(),
                Filter = new List<IQuery> { null },
            },
        };

        private static readonly Query ExpectedValidQueryMissingDefaultField = new Query
        {
            Bool = new BoolQuery
            {
                Must = new List<IQuery> {
                    new QueryStringClause()
                    {
                        Phrase = "TEST_FIELD:[0 TO 2]",
                        Wildcard = true,
                        Default = "*",
                    },
                },
                MustNot = new List<IQuery>(),
                Should = new List<IQuery>(),
                ShouldNot = new List<IQuery>(),
                Filter = new List<IQuery> { null },
            },
        };

        private static readonly object[] QueryStringClauseTestCases = {
            new TestCaseData(ValidQuery, ExpectedValidQuery).SetName("JsonDeserializeObject_WithValidQueryClause_DeserializedCorrectly"),
            new TestCaseData(QueryMissingAnalyzeWildcardProperty, ExpectedValidQueryMissingAnalyzeWildcard).SetName("JsonDeserializeObject_WithQueryMissingAnalyzeWildcardProperty_DeserializedCorrectly"),
            new TestCaseData(QueryMissingDefaultFieldProperty, ExpectedValidQueryMissingDefaultField).SetName("JsonDeserializeObject_WithQueryMissingDefaultFieldProperty_DeserializedCorrectly"),
        };

        [TestCaseSource(nameof(QueryStringClauseTestCases))]
        public void TestQueryStringQueries(string queryString, object expected)
        {
            queryString.AssertJsonString((Query)expected);
        }
    }
}