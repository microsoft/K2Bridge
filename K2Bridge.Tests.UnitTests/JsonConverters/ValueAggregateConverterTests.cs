// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2Bridge.Tests.UnitTests.JsonConverters
{
    using K2Bridge.Models.Response.Aggregations;
    using NUnit.Framework;

    [TestFixture]
    public class ValueAggregateConverterTests
    {
        private const string ExpectedValidValueAggregateWithValue = @"{
            ""value"": 644.0861
        }";

        private const string ExpectedValidValueAggregateWithValueAsString = @"{
            ""value"": 1625176800000.0,
            ""value_as_string"": ""2021-07-02T00:00:00.000+02:00""
        }";

        private const string ExpectedValidValueAggregateWithNullValues = @"{
            ""value"": null
        }";

        private static readonly ValueAggregate ValidValueAggregateWithValue = new()
        {
            Value = 644.0861,
        };

        private static readonly ValueAggregate ValidValueAggregateWithValueAsString = new()
        {
            Value = 1625176800000,
            ValueAsString = "2021-07-02T00:00:00.000+02:00",
        };

        private static readonly ValueAggregate ValidValueAggregateWithNullValues = new()
        {
            Value = null,
            ValueAsString = null,
        };

        private static readonly object[] ValueAggregateTestCases = {
            new TestCaseData(ExpectedValidValueAggregateWithValue, ValidValueAggregateWithValue).SetName("JsonDeserialize_WithValidValueAggregateWithValue_DeserializedCorrectly"),
            new TestCaseData(ExpectedValidValueAggregateWithValueAsString, ValidValueAggregateWithValueAsString).SetName("JsonDeserialize_WithValidValueAggregateWithValueAsString_DeserializedCorrectly"),
            new TestCaseData(ExpectedValidValueAggregateWithNullValues, ValidValueAggregateWithNullValues).SetName("JsonDeserialize_WithValidValueAggregateWithValueAsString_DeserializedCorrectly"),
        };

        [TestCaseSource(nameof(ValueAggregateTestCases))]
        public void TestValueAggregateConverter(string queryString, object expected)
        {
            ((ValueAggregate)expected).AssertJson(queryString);
        }
    }
}
