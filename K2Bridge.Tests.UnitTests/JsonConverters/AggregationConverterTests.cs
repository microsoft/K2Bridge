// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace UnitTests.K2Bridge.JsonConverters
{
    using System.Collections.Generic;
    using global::K2Bridge.Models.Request.Aggregations;
    using NUnit.Framework;

    [TestFixture]
    public class AggregationConverterTests
    {
        private const string DateHistogramAggregation = @"
            {""aggs"": { 
                ""2"": {
                    ""date_histogram"": {
                        ""field"": ""timestamp"",
                        ""fixed_interval"": ""1m"",
                        ""time_zone"": ""Asia/Jerusalem"",
                        ""min_doc_count"": 1
                    }
                }
            }}";

        private const string TermsAggregation = @"
            {""aggs"": {
                ""2"": {
                    ""terms"": {
                        ""field"": ""DestCountry"",
                        ""order"": {
                            ""_count"": ""desc""
                        },
                        ""size"": 10
                    }
                }
            }}";

        private const string CardinalityAggregation = @"
            {""aggs"": { 
                ""2"": {
                    ""cardinality"": {
                        ""field"": ""metric"",
                    }
                }
            }}";

        private const string AvgAggregation = @"
            {""aggs"": { 
                ""2"": {
                    ""avg"" : { 
                        ""field"" : ""metric"" 
                    } 
                }
            }}";

        private const string AvgEmptyFieldsAggregation = @"
            {""aggs"": { 
                ""2"": {
                    ""avg"" : { 
                        ""nofield"" : ""metric"" 
                    } 
                }
            }}";

        private const string SumAggregation = @"
            {""aggs"": { 
                ""2"": {
                    ""sum"" : {
                        ""field"" : ""metric"" 
                    } 
                }
            }}";

        private const string NoAggAggregation = @"
            {""aggs"": { 
                ""2"": {
                    ""noagg"" : { 
                        ""field"" : ""metric"" 
                        } 
                }
            }}";

        private static readonly AggregationContainer ExpectedValidDateHistogramAggregation = new AggregationContainer()
        {
            PrimaryAggregation = null,
            SubAggregations = new AggregationDictionary
            {
                ["2"] = new AggregationContainer
                {
                    PrimaryAggregation = new DateHistogramAggregation
                    {
                        Field = "timestamp",
                        Key = "2",
                        FixedInterval = "1m",
                        TimeZone = "Asia/Jerusalem",
                        MinimumDocumentCount = 1,
                    },
                    SubAggregations = new AggregationDictionary(),
                },
            },
        };

        private static readonly AggregationContainer ExpectedValidTermsAggregation = new AggregationContainer()
        {
            PrimaryAggregation = null,
            SubAggregations = new AggregationDictionary
            {
                ["2"] = new AggregationContainer
                {
                    PrimaryAggregation = new TermsAggregation
                    {
                        Field = "DestCountry",
                        Key = "2",
                        Order = new TermsOrder
                        {
                            SortField = "_count",
                            SortOrder = "desc",
                        },
                    },
                    SubAggregations = new AggregationDictionary(),
                },
            },
        };

        private static readonly AggregationContainer ExpectedValidCardinalityAggregation = new AggregationContainer()
        {
            PrimaryAggregation = null,
            SubAggregations = new AggregationDictionary
            {
                ["2"] = new AggregationContainer
                {
                    PrimaryAggregation = new CardinalityAggregation
                    {
                        Field = "metric",
                        Key = "2",
                    },
                    SubAggregations = new AggregationDictionary(),
                },
            },
        };

        private static readonly AggregationContainer ExpectedValidAvgAggregation = new AggregationContainer()
        {
            PrimaryAggregation = null,
            SubAggregations = new AggregationDictionary
            {
                ["2"] = new AggregationContainer
                {
                    PrimaryAggregation = new AverageAggregation
                    {
                        Field = "metric",
                        Key = "2",
                    },
                    SubAggregations = new AggregationDictionary(),
                },
            },
        };

        private static readonly AggregationContainer ExpectedNoFieldsAvgAggregation = new AggregationContainer()
        {
            PrimaryAggregation = null,
            SubAggregations = new AggregationDictionary
            {
                ["2"] = new AggregationContainer
                {
                    PrimaryAggregation = new AverageAggregation
                    {
                        Field = null,
                        Key = "2",
                    },
                    SubAggregations = new AggregationDictionary(),
                },
            },
        };

        private static readonly AggregationContainer ExpectedValidSumAggregation = new AggregationContainer()
        {
            PrimaryAggregation = null,
            SubAggregations = new AggregationDictionary
            {
                ["2"] = new AggregationContainer
                {
                    PrimaryAggregation = new SumAggregation
                    {
                        Field = "metric",
                        Key = "2",
                    },
                    SubAggregations = new AggregationDictionary(),
                },
            },
        };

        private static readonly AggregationContainer ExpectedNoAggAggregation = new AggregationContainer()
        {
            PrimaryAggregation = null,
            SubAggregations = new AggregationDictionary
            {
                ["2"] = new AggregationContainer
                {
                    PrimaryAggregation = null,
                    SubAggregations = new AggregationDictionary(),
                },
            },
        };

        private static readonly object[] AggregationTestCases = {
            new TestCaseData(DateHistogramAggregation, ExpectedValidDateHistogramAggregation).SetName("JsonDeserializeObject_WithAggregationValidDateHistogram_DeserializedCorrectly"),
            new TestCaseData(TermsAggregation, ExpectedValidTermsAggregation).SetName("JsonDeserializeObject_WithAggregationValidTerms_DeserializedCorrectly"),
            new TestCaseData(CardinalityAggregation, ExpectedValidCardinalityAggregation).SetName("JsonDeserializeObject_WithAggregationValidCardinality_DeserializedCorrectly"),
            new TestCaseData(AvgAggregation, ExpectedValidAvgAggregation).SetName("JsonDeserializeObject_WithAggregationValidAvg_DeserializedCorrectly"),
            new TestCaseData(AvgEmptyFieldsAggregation, ExpectedNoFieldsAvgAggregation).SetName("JsonDeserializeObject_WithAggregationNoFieldsAvg_DeserializedCorrectly"),
            new TestCaseData(SumAggregation, ExpectedValidSumAggregation).SetName("JsonDeserializeObject_WithAggregationValidSum_DeserializedCorrectly"),
            new TestCaseData(NoAggAggregation, ExpectedNoAggAggregation).SetName("JsonDeserializeObject_WithNoAgg_DeserializedCorrectly"),
        };

        [TestCaseSource(nameof(AggregationTestCases))]
        public void TestAggregationConverter(string queryString, object expected)
        {
            queryString.AssertJsonString((AggregationContainer)expected);
        }
    }
}
