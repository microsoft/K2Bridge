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
                    ""avg"" : { ""field"" : ""grade"" } 
                }
            }}";

        private const string AvgEmptyFieldsAggregation = @"
        {""aggs"": { 
            ""2"": {
                ""avg"" : { ""nofield"" : ""grade"" } 
            }
        }}";

        private const string NoAggAggregation = @"
        {""aggs"": { 
            ""2"": {
                ""noagg"" : { ""field"" : ""grade"" } 
            }
        }}";

        private static readonly Aggregation ExpectedValidDateHistogramAggregation = new Aggregation()
        {
            PrimaryAggregation = null,
            SubAggregations = new Dictionary<string, Aggregation>
                {
                    { "2", new Aggregation() {
                        PrimaryAggregation = new DateHistogramAggregation {
                            FieldName = "timestamp",
                            FieldAlias = "_2",
                            Interval = "1m",
                            Metric = "count()",
                            },
                        SubAggregations = new Dictionary<string, Aggregation>(),
                        }
                    },
                },
        };

        private static readonly Aggregation ExpectedValidCardinalityAggregation = new Aggregation()
        {
            PrimaryAggregation = null,
            SubAggregations = new Dictionary<string, Aggregation>
                {
                    { "2", new Aggregation() {
                        PrimaryAggregation = new CardinalityAggregation {
                            FieldAlias = "_2",
                            FieldName = "metric",
                            },
                        SubAggregations = new Dictionary<string, Aggregation>(),
                        }
                    },
                },
        };

        private static readonly Aggregation ExpectedValidAvgAggregation = new Aggregation()
        {
            PrimaryAggregation = null,
            SubAggregations = new Dictionary<string, Aggregation>
                {
                    { "2", new Aggregation() {
                        PrimaryAggregation = new AvgAggregation {
                            FieldAlias = "_2",
                            FieldName = "grade",
                            },
                        SubAggregations = new Dictionary<string, Aggregation>(),
                        }
                    },
                },
        };

        private static readonly Aggregation ExpectedNoFieldsAvgAggregation = new Aggregation()
        {
            PrimaryAggregation = null,
            SubAggregations = new Dictionary<string, Aggregation>
                {
                    { "2", new Aggregation() {
                        PrimaryAggregation = new AvgAggregation {
                            FieldAlias = "_2",
                            FieldName = null,
                            },
                        SubAggregations = new Dictionary<string, Aggregation>(),
                        }
                    },
                },
        };

        private static readonly Aggregation ExpectedNoAggAggregation = new Aggregation()
        {
            PrimaryAggregation = null,
            SubAggregations = new Dictionary<string, Aggregation>
                {
                    { "2", new Aggregation() {
                        PrimaryAggregation = null,
                        SubAggregations = new Dictionary<string, Aggregation>(),
                        }
                    },
                },
        };

        private static readonly object[] AggregationTestCases = {
            new TestCaseData(DateHistogramAggregation, ExpectedValidDateHistogramAggregation).SetName("JsonDeserializeObject_WithAggregationValidDateHistogram_DeserializedCorrectly"),
            new TestCaseData(CardinalityAggregation, ExpectedValidCardinalityAggregation).SetName("JsonDeserializeObject_WithAggregationValidCardinality_DeserializedCorrectly"),
            new TestCaseData(AvgAggregation, ExpectedValidAvgAggregation).SetName("JsonDeserializeObject_WithAggregationValidAvg_DeserializedCorrectly"),
            new TestCaseData(AvgEmptyFieldsAggregation, ExpectedNoFieldsAvgAggregation).SetName("JsonDeserializeObject_WithAggregationNoFieldsAvg_DeserializedCorrectly"),
            new TestCaseData(NoAggAggregation, ExpectedNoAggAggregation).SetName("JsonDeserializeObject_WithNoAgg_DeserializedCorrectly"),
        };

        [TestCaseSource(nameof(AggregationTestCases))]
        public void TestAggregationConverter(string queryString, object expected)
        {
            queryString.AssertJsonString((Aggregation)expected);
        }
    }
}
