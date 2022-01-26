// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace UnitTests.K2Bridge.JsonConverters
{
    using System.Collections.Generic;
    using global::K2Bridge.Models.Request;
    using global::K2Bridge.Models.Request.Aggregations;
    using NUnit.Framework;

    [TestFixture]
    public class AggregationContainerConverterTests
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

        private const string RangeAggregation = @"
            {""aggs"": {
                ""2"": {
                    ""range"": {
                        ""field"": ""DestCountry"",
                        ""ranges"": [{
                            ""from"": 0,
                            ""to"": 100
                        }],
                        ""keyed"": true
                    }
                }
            }}";

        private const string DateRangeAggregation = @"
            {""aggs"": {
                ""2"": {
                    ""date_range"": {
                        ""field"": ""timestamp"",
                        ""ranges"": [{
                            ""from"": ""2016-02-01"",
                            ""to"": ""now/d""
                        }]
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

        private const string HistogramAggregation = @"
            {""aggs"": {
                ""2"": {
                    ""histogram"": {
                        ""field"": ""price"",
                        ""interval"": 50,
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

        private const string MinAggregation = @"
            {""aggs"": {
                ""2"": {
                    ""min"" : {
                        ""field"" : ""metric""
                    }
                }
            }}";

        private const string MaxAggregation = @"
            {""aggs"": {
                ""2"": {
                    ""max"" : {
                        ""field"" : ""metric""
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

        private const string PercentileAggregation = @"
            {""aggs"": {
                ""2"": {
                    ""percentiles"": {
                        ""field"": ""metric"",
                        ""percents"": [
                            50
                        ]
                    }
                }
            }}";

        private const string PercentilesAggregation = @"
            {""aggs"": {
                ""2"": {
                    ""percentiles"": {
                        ""field"": ""metric"",
                        ""percents"": [
                            1, 50, 90, 95
                        ]
                    }
                }
            }}";

        private const string PercentilesKeyedAggregation = @"
            {""aggs"": {
                ""2"": {
                    ""percentiles"": {
                        ""field"": ""metric"",
                        ""keyed"": ""true"",
                        ""percents"": [
                            1, 50, 90, 95
                        ]
                    }
                }
            }}";

        private const string ExtendedStatsAggregationWithSigma = @"
            {""aggs"": {
                ""2"": {
                    ""extended_stats"" : {
                        ""field"" : ""metric"",
                        ""sigma"" : 3
                    }
                }
            }}";

        private const string ExtendedStatsAggregationWithoutSigma = @"
            {""aggs"": {
                ""2"": {
                    ""extended_stats"" : {
                        ""field"" : ""metric""
                    }
                }
            }}";

        private const string TopHitsAggregation = @"
            {""aggs"": {
                ""2"": {
                    ""top_hits"": {
                        ""docvalue_fields"": [
                            {
                                ""field"": ""metricfield""
                            }
                        ],
                        ""size"": 1,
                        ""sort"": [
                            {
                                ""sortfield"": {
                                    ""order"": ""desc""
                                }
                            }
                        ]
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
                        Metric = "count()",
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
                        Metric = "count()",
                    },
                    SubAggregations = new AggregationDictionary(),
                },
            },
        };

        private static readonly AggregationContainer ExpectedValidRangeAggregation = new AggregationContainer()
        {
            PrimaryAggregation = null,
            SubAggregations = new AggregationDictionary
            {
                ["2"] = new AggregationContainer
                {
                    PrimaryAggregation = new RangeAggregation
                    {
                        Field = "DestCountry",
                        Key = "2",
                        Keyed = true,
                        Ranges = new List<RangeAggregationExpression>() {
                            new RangeAggregationExpression()
                            {
                                From = 0,
                                To = 100,
                            },
                        },
                        Metric = "count()",
                    },
                    SubAggregations = new AggregationDictionary(),
                },
            },
        };

        private static readonly AggregationContainer ExpectedValidDateRangeAggregation = new AggregationContainer()
        {
            PrimaryAggregation = null,
            SubAggregations = new AggregationDictionary
            {
                ["2"] = new AggregationContainer
                {
                    PrimaryAggregation = new DateRangeAggregation
                    {
                        Field = "DestCountry",
                        Key = "2",
                        Keyed = false,
                        Ranges = new List<DateRangeAggregationExpression>() {
                            new DateRangeAggregationExpression()
                            {
                                From = "2016-02-01",
                                To = "now/d",
                            },
                        },
                    },
                },
            },
        };

        private static readonly AggregationContainer ExpectedValidHistogramAggregation = new AggregationContainer()
        {
            PrimaryAggregation = null,
            SubAggregations = new AggregationDictionary
            {
                ["2"] = new AggregationContainer
                {
                    PrimaryAggregation = new HistogramAggregation
                    {
                        Field = "price",
                        Key = "2",
                        Interval = 50,
                        MinimumDocumentCount = 1,
                        Metric = "count()",
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

        private static readonly AggregationContainer ExpectedValidMinAggregation = new AggregationContainer()
        {
            PrimaryAggregation = null,
            SubAggregations = new AggregationDictionary
            {
                ["2"] = new AggregationContainer
                {
                    PrimaryAggregation = new MinAggregation
                    {
                        Field = "metric",
                        Key = "2",
                    },
                    SubAggregations = new AggregationDictionary(),
                },
            },
        };

        private static readonly AggregationContainer ExpectedValidMaxAggregation = new AggregationContainer()
        {
            PrimaryAggregation = null,
            SubAggregations = new AggregationDictionary
            {
                ["2"] = new AggregationContainer
                {
                    PrimaryAggregation = new MaxAggregation
                    {
                        Field = "metric",
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

        private static readonly AggregationContainer ExpectedValidPercentileAggregation = new AggregationContainer()
        {
            PrimaryAggregation = null,
            SubAggregations = new AggregationDictionary
            {
                ["2"] = new AggregationContainer()
                {
                    PrimaryAggregation = new PercentileAggregation
                    {
                        Field = "metric",
                        Key = "2",
                        Percents = new double[] { 50 },
                    },
                    SubAggregations = new AggregationDictionary(),
                },
            },
        };

        private static readonly AggregationContainer ExpectedValidPercentilesAggregation = new AggregationContainer()
        {
            PrimaryAggregation = null,
            SubAggregations = new AggregationDictionary
            {
                ["2"] = new AggregationContainer()
                {
                    PrimaryAggregation = new PercentileAggregation
                    {
                        Field = "metric",
                        Key = "2",
                        Percents = new double[] { 1, 50, 90, 95 },
                    },
                    SubAggregations = new AggregationDictionary(),
                },
            },
        };

        private static readonly AggregationContainer ExpectedValidPercentilesKeyedAggregation = new AggregationContainer()
        {
            PrimaryAggregation = null,
            SubAggregations = new AggregationDictionary
            {
                ["2"] = new AggregationContainer()
                {
                    PrimaryAggregation = new PercentileAggregation
                    {
                        Field = "metric",
                        Key = "2",
                        Keyed = true,
                        Percents = new double[] { 1, 50, 90, 95 },
                    },
                    SubAggregations = new AggregationDictionary(),
                },
            },
        };

        private static readonly AggregationContainer ExpectedExtendedStatsAggregationWithoutSigma = new AggregationContainer()
        {
            PrimaryAggregation = null,
            SubAggregations = new AggregationDictionary
            {
                ["2"] = new AggregationContainer
                {
                    PrimaryAggregation = new ExtendedStatsAggregation
                    {
                        Field = "metric",
                        Key = "2",
                    },
                    SubAggregations = new AggregationDictionary(),
                },
            },
        };

        private static readonly AggregationContainer ExpectedExtendedStatsAggregationWithSigma = new AggregationContainer()
        {
            PrimaryAggregation = null,
            SubAggregations = new AggregationDictionary
            {
                ["2"] = new AggregationContainer
                {
                    PrimaryAggregation = new ExtendedStatsAggregation
                    {
                        Field = "metric",
                        Key = "2",
                        Sigma = 3,
                    },
                    SubAggregations = new AggregationDictionary(),
                },
            },
        };

        private static readonly AggregationContainer ExpectedTopHitsAggregation = new AggregationContainer()
        {
            PrimaryAggregation = null,
            SubAggregations = new AggregationDictionary
            {
                ["2"] = new AggregationContainer
                {
                    PrimaryAggregation = new TopHitsAggregation
                    {
                        DocValueFields = new List<DocValueField>() { new DocValueField() { Field = "metricfield" } },
                        Key = "2",
                        Field = "metricfield",
                        Size = 1,
                        Sort = new List<SortClause>() { new SortClause() { FieldName = "sortfield", Order = "desc" } },
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
            new TestCaseData(HistogramAggregation, ExpectedValidHistogramAggregation).SetName("JsonDeserializeObject_WithAggregationValidHistogram_DeserializedCorrectly"),
            new TestCaseData(TermsAggregation, ExpectedValidTermsAggregation).SetName("JsonDeserializeObject_WithAggregationValidTerms_DeserializedCorrectly"),
            new TestCaseData(RangeAggregation, ExpectedValidRangeAggregation).SetName("JsonDeserializeObject_WithAggregationValidRange_DeserializedCorrectly"),
            new TestCaseData(CardinalityAggregation, ExpectedValidCardinalityAggregation).SetName("JsonDeserializeObject_WithAggregationValidCardinality_DeserializedCorrectly"),
            new TestCaseData(AvgAggregation, ExpectedValidAvgAggregation).SetName("JsonDeserializeObject_WithAggregationValidAvg_DeserializedCorrectly"),
            new TestCaseData(AvgEmptyFieldsAggregation, ExpectedNoFieldsAvgAggregation).SetName("JsonDeserializeObject_WithAggregationNoFieldsAvg_DeserializedCorrectly"),
            new TestCaseData(MinAggregation, ExpectedValidMinAggregation).SetName("JsonDeserializeObject_WithAggregationValidMin_DeserializedCorrectly"),
            new TestCaseData(MaxAggregation, ExpectedValidMaxAggregation).SetName("JsonDeserializeObject_WithAggregationValidMax_DeserializedCorrectly"),
            new TestCaseData(SumAggregation, ExpectedValidSumAggregation).SetName("JsonDeserializeObject_WithAggregationValidSum_DeserializedCorrectly"),
            new TestCaseData(PercentileAggregation, ExpectedValidPercentileAggregation).SetName("JsonDeserializeObject_WithAggregationValidPercentile_DeserializedCorrectly"),
            new TestCaseData(PercentilesAggregation, ExpectedValidPercentilesAggregation).SetName("JsonDeserializeObject_WithAggregationValidPercentiles_DeserializedCorrectly"),
            new TestCaseData(PercentilesKeyedAggregation, ExpectedValidPercentilesKeyedAggregation).SetName("JsonDeserializeObject_WithAggregationValidPercentiles_DeserializedCorrectly"),
            new TestCaseData(ExtendedStatsAggregationWithSigma, ExpectedExtendedStatsAggregationWithSigma).SetName("JsonDeserializeObject_WithExtendedStatsAggregationWithSigma_DeserializedCorrectly"),
            new TestCaseData(ExtendedStatsAggregationWithoutSigma, ExpectedExtendedStatsAggregationWithoutSigma).SetName("JsonDeserializeObject_WithExtendedStatsAggregationWithoutSigma_DeserializedCorrectly"),
            new TestCaseData(TopHitsAggregation, ExpectedTopHitsAggregation).SetName("JsonDeserializeObject_WithTopHitsAggregation_DeserializedCorrectly"),
            new TestCaseData(NoAggAggregation, ExpectedNoAggAggregation).SetName("JsonDeserializeObject_WithNoAgg_DeserializedCorrectly"),
        };

        [TestCaseSource(nameof(AggregationTestCases))]
        public void TestAggregationConverter(string queryString, object expected)
        {
            queryString.AssertJsonString((AggregationContainer)expected);
        }
    }
}
