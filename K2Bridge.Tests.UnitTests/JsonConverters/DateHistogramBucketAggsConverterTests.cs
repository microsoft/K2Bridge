// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace UnitTests.K2Bridge.JsonConverters
{
    using System.Collections.Generic;
    using global::K2Bridge.Models.Response;
    using global::K2Bridge.Models.Response.Metadata;
    using NUnit.Framework;

    [TestFixture]
    public class DateHistogramBucketAggsConverterTests
    {
        private const string ExpectedValidBucket = @"{
            ""doc_count"": 502,
            ""key"": 42,
            ""key_as_string"": ""foo""
        }";

        private const string ExpectedValidBucketWithAggs = @"{
            ""doc_count"": 502,
            ""key"": 42,
            ""key_as_string"": ""foo"",
            ""aggs"": { ""values"" : {""50.0"": 644.0861} }
        }";

        private static readonly DateHistogramBucket ValidTermsBucket = new DateHistogramBucket()
        {
            DocCount = 502,
            Key = 42,
            KeyAsString = "foo",
            Aggs = new Dictionary<string, Dictionary<string, double>>(),
        };

        private static readonly DateHistogramBucket ValidTermsBucketWithAggs = new DateHistogramBucket()
        {
            DocCount = 502,
            Key = 42,
            KeyAsString = "foo",
            Aggs = new Dictionary<string, Dictionary<string, double>>() {
                { "aggs", new Dictionary<string, double> { { "50.0", 644.0861 } } },
            },
        };

        private static readonly object[] FieldCapsTestCases = {
            new TestCaseData(ExpectedValidBucket, ValidTermsBucket).SetName("JsonDeserialize_WithValidTermsBucket_DeserializedCorrectly"),
            new TestCaseData(ExpectedValidBucketWithAggs, ValidTermsBucketWithAggs).SetName("JsonDeserialize_WithValidTermsBucketWithAggs_DeserializedCorrectly"),
        };

        [TestCaseSource(nameof(FieldCapsTestCases))]
        public void TestTermsBucketAggsConverter(string queryString, object expected)
        {
            ((DateHistogramBucket)expected).AssertJson(queryString);
        }
    }
}