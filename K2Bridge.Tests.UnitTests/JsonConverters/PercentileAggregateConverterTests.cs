// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2Bridge.Tests.UnitTests.JsonConverters
{
    using K2Bridge.Models.Response.Aggregations;
    using NUnit.Framework;

    [TestFixture]
    public class PercentileAggregateConverterTests
    {
        private const string ExpectedValidPercentileAggregateWithKeyedFalse = @"{
            ""values"": [
                {
                    ""key"": 25.0,
                    ""value"": 1626686619141.875,
                    ""value_as_string"": ""2021-07-19T09:23:39.141Z""
                },
                {
                    ""key"": 50.0,
                    ""value"": 1626829958440.0,
                    ""value_as_string"": ""2021-07-21T01:12:38.440Z""
                },
                {
                    ""key"": 75.0,
                    ""value"": 1630222036110.0,
                    ""value_as_string"": ""2021-08-29T07:27:16.110Z""
                }
            ]
        }";

        private const string ExpectedValidPercentileAggregateWithKeyedTrue = @"{
            ""values"": {
                ""25.0"": 1626686619141.875,
                ""25.0_as_string"": ""2021-07-19T09:23:39.141Z"",
                ""50.0"": 1626829958440.0,
                ""50.0_as_string"": ""2021-07-21T01:12:38.440Z"",
                ""75.0"": 1630222036110.0,
                ""75.0_as_string"": ""2021-08-29T07:27:16.110Z""
            }
        }";

        private const string ExpectedValidPercentileAggregateWithKeyedFalseAndNullValues = @"{
            ""values"": [
                {
                    ""key"": 25.0,
                    ""value"": null
                },
                {
                    ""key"": 50.0,
                    ""value"": null
                },
                {
                    ""key"": 75.0,
                    ""value"": null
                }
            ]
        }";

        private const string ExpectedValidPercentileAggregateWithKeyedTrueAndNullValues = @"{
            ""values"": {
                ""25.0"": null,
                ""50.0"": null,
                ""75.0"": null
            }
        }";

        private static readonly PercentileAggregate ValidPercentileAggregateWithKeyedFalse = new()
        {
            Keyed = false,
        };

        private static readonly PercentileAggregate ValidPercentileAggregateWithKeyedTrue = new()
        {
            Keyed = true,
        };

        private static readonly object[] PercentileAggregateTestCases = {
            new TestCaseData(ExpectedValidPercentileAggregateWithKeyedFalse, ValidPercentileAggregateWithKeyedFalse).SetName("JsonDeserialize_WithValidPercentileAggregateWithKeyedFalse_DeserializedCorrectly"),
            new TestCaseData(ExpectedValidPercentileAggregateWithKeyedTrue, ValidPercentileAggregateWithKeyedTrue).SetName("JsonDeserialize_WithValidPercentileAggregateWithKeyedFalse_DeserializedCorrectly"),
        };

        private static readonly object[] PercentileAggregateWithNullValuesTestCases = {
            new TestCaseData(ExpectedValidPercentileAggregateWithKeyedFalseAndNullValues, ValidPercentileAggregateWithKeyedFalse).SetName("JsonDeserialize_WithValidPercentileAggregateWithKeyedFalseAndNullValues_DeserializedCorrectly"),
            new TestCaseData(ExpectedValidPercentileAggregateWithKeyedTrueAndNullValues, ValidPercentileAggregateWithKeyedTrue).SetName("JsonDeserialize_WithValidPercentileAggregateWithKeyedFalseAndNullValues_DeserializedCorrectly"),
        };

        [TestCaseSource(nameof(PercentileAggregateTestCases))]
        public void TestPercentileAggregateConverter(string queryString, PercentileAggregate expected)
        {
            expected.Values.RemoveAll(item => true);
            expected.Values.Add(new PercentileItem()
            {
                Percentile = 25,
                Value = 1626686619141.875,
                ValueAsString = "2021-07-19T09:23:39.141Z",
            });
            expected.Values.Add(new PercentileItem()
            {
                Percentile = 50,
                Value = 1626829958440,
                ValueAsString = "2021-07-21T01:12:38.440Z",
            });
            expected.Values.Add(new PercentileItem()
            {
                Percentile = 75,
                Value = 1630222036110,
                ValueAsString = "2021-08-29T07:27:16.110Z",
            });

            expected.AssertJson(queryString);
        }

        [TestCaseSource(nameof(PercentileAggregateWithNullValuesTestCases))]
        public void TestPercentileAggregateConverterWithNullValues(string queryString, PercentileAggregate expected)
        {
            expected.Values.RemoveAll(item => true);
            expected.Values.Add(new PercentileItem()
            {
                Percentile = 25,
                Value = null,
                ValueAsString = null,
            });
            expected.Values.Add(new PercentileItem()
            {
                Percentile = 50,
                Value = null,
                ValueAsString = null,
            });
            expected.Values.Add(new PercentileItem()
            {
                Percentile = 75,
                Value = null,
                ValueAsString = null,
            });

            expected.AssertJson(queryString);
        }
    }
}
