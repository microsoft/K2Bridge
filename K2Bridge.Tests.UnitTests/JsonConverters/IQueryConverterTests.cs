// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace UnitTests.K2Bridge.JsonConverters
{
    using System.Collections.Generic;
    using DeepEqual.Syntax;
    using global::K2Bridge.Models.Request;
    using global::K2Bridge.Models.Request.Queries;
    using Newtonsoft.Json;
    using NUnit.Framework;

    [TestFixture]
    public partial class JsonConvertersTests
    {
        private const string OutterBoolQuery = @"
              {""bool"":
                  {""must"":
                      [
                      ],
                      ""filter"":
                      [
                          {""match_all"":{}}
                      ],
                      ""should"":[],
                      ""must_not"":[]
                  }
              }";

        private const string InnerBoolQuery = @"
        {
           ""bool"": {
             ""must"": [
               {
                 ""query_string"": {
                    ""query"": ""MITZI"",
                    ""analyze_wildcard"": true,
                    ""default_field"": ""-""
                 }
               },
               {
                 ""bool"": {
                   ""should"": [
                     {
                       ""match_phrase"": {
                         ""DestWeather"": ""cloudy""
                       }
                     }
                   ],
                   ""must"": [
                     {
                       ""match_phrase"": {
                         ""DestWeather"": ""sunny""
                       }
                     }
                   ],
                   ""must_not"": [
                     {
                       ""match_phrase"": {
                         ""DestWeather"": ""rainy""
                       }
                     }
                   ],
                   ""minimum_should_match"": 1
                 }
               },
               {
                 ""range"": {
                   ""timestamp"": {
                        ""gte"": 1581963795598,
                        ""lte"": 1581964695598,
                        ""format"": ""epoch_millis""
                   }
                 }
               }
             ],
             ""filter"": [],
             ""should"": [],
             ""must_not"": []
             }
        }";

        private const string LeafClause = @"
        {
            ""range"": {
              ""timestamp"": {
                    ""gte"": 1581963795598,
                    ""lte"": 1581964695598,
                    ""format"": ""epoch_millis""
              }
            }
        }";

        private static readonly BoolQuery OutterBoolResult = new BoolQuery
        {
            Must = new List<IQuery>(),
            MustNot = new List<IQuery>(),
            Should = new List<IQuery>(),
            ShouldNot = new List<IQuery>(),
            Filter = new List<IQuery> { null },
        };

        private static readonly BoolQuery InnerBoolResult = new BoolQuery
        {
            Must = new List<IQuery>
            {
                new QueryStringClause()
                {
                    Default = "-",
                    Wildcard = true,
                    Phrase = "MITZI",
                },
                new BoolQuery()
                {
                    Must = new List<IQuery>
                    {
                        new MatchPhraseClause()
                        {
                            FieldName = "DestWeather",
                            Phrase = "sunny",
                        },
                    },
                    MustNot = new List<IQuery>
                    {
                        new MatchPhraseClause()
                        {
                            FieldName = "DestWeather",
                            Phrase = "rainy",
                        },
                    },
                    Should = new List<IQuery> {
                        new MatchPhraseClause()
                        {
                            FieldName = "DestWeather",
                            Phrase = "cloudy",
                        },
                    },
                    ShouldNot = new List<IQuery>(),
                    Filter = new List<IQuery> { },
                },
                new RangeClause()
                {
                    FieldName = "timestamp",
                    GTEValue = 1581963795598,
                    LTEValue = 1581964695598,
                    Format = "epoch_millis",
                },
            },
            MustNot = new List<IQuery>(),
            Should = new List<IQuery>(),
            ShouldNot = new List<IQuery>(),
            Filter = new List<IQuery> { },
        };

        private static readonly RangeClause LeafResult = new RangeClause()
        {
            FieldName = "timestamp",
            GTEValue = 1581963795598,
            LTEValue = 1581964695598,
            Format = "epoch_millis",
        };

        private static readonly object[] QueryTestCases = {
            new TestCaseData(OutterBoolQuery, OutterBoolResult).SetName("JsonDeserializeObject_WithIQueryOutterBool_DeserializedCorrectly"),
            new TestCaseData(InnerBoolQuery, InnerBoolResult).SetName("JsonDeserializeObject_WithIQueryInnerBool_DeserializedCorrectly"),
            new TestCaseData(LeafClause, LeafResult).SetName("JsonDeserializeObject_WithIQueryLeafClause_DeserializedCorrectly"),
        };

        [TestCaseSource(nameof(QueryTestCases))]
        public void ReadQueryAndValidate(string queryString, object expected)
        {
            queryString.AssertJsonString((IQuery)expected);
        }
    }
}