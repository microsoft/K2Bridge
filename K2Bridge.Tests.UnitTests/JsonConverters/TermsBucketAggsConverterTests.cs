// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace UnitTests.K2Bridge.JsonConverters
{
    using global::K2Bridge.Models.Response.Aggregations;
    using NUnit.Framework;

    [TestFixture]
    public class TermsBucketAggsConverterTests
    {
        private const string ExpectedValidBucket = @"{
            ""doc_count"": 502,
            ""key"": ""IT""
        }";

        private static readonly TermsBucket ValidTermsBucket = new TermsBucket()
        {
            DocCount = 502,
            Key = "IT",
        };

        private static readonly object[] FieldCapsTestCases = {
            new TestCaseData(ExpectedValidBucket, ValidTermsBucket).SetName("JsonDeserialize_WithValidTermsBucket_DeserializedCorrectly"),
        };

        [TestCaseSource(nameof(FieldCapsTestCases))]
        public void TestTermsBucketAggsConverter(string queryString, object expected)
        {
            ((TermsBucket)expected).AssertJson(queryString);
        }
    }
}