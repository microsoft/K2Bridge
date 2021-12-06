// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace UnitTests.K2Bridge.JsonConverters
{
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

        private static readonly DateHistogramBucket ValidTermsBucket = new DateHistogramBucket()
        {
            DocCount = 502,
            Key = 42,
            KeyAsString = "foo",
            Aggs = new System.Collections.Generic.Dictionary<string, System.Collections.Generic.List<double>>(),
        };

        private static readonly object[] FieldCapsTestCases = {
            new TestCaseData(ExpectedValidBucket, ValidTermsBucket).SetName("JsonDeserialize_WithValidTermsBucket_DeserializedCorrectly"),
        };

        [TestCaseSource(nameof(FieldCapsTestCases))]
        public void TestTermsBucketAggsConverter(string queryString, object expected)
        {
            ((DateHistogramBucket)expected).AssertJson(queryString);
        }
    }
}