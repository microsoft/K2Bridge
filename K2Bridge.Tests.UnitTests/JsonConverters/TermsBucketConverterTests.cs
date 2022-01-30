// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2Bridge.Tests.UnitTests.JsonConverters
{
    using K2Bridge.Models.Response.Aggregations;
    using K2Bridge.Models.Response.Aggregations.Bucket;
    using NUnit.Framework;

    [TestFixture]
    public class TermsBucketConverterTests
    {
        private const string ExpectedValidBucket = @"{
            ""doc_count"": 502,
            ""key"": ""IT""
        }";

        private const string ExpectedValidBucketWithAggs = @"{
            ""doc_count"": 502,
            ""key"": ""IT"",
            ""2"" : {""value"": 644.0861}
        }";

        private static readonly TermsBucket ValidTermsBucket = new()
        {
            DocCount = 502,
            Key = "IT",
        };

        private static readonly object[] FieldCapsTestCases = {
            new TestCaseData(ExpectedValidBucket, ValidTermsBucket).SetName("JsonDeserialize_WithValidTermsBucket_DeserializedCorrectly"),
            new TestCaseData(ExpectedValidBucketWithAggs, ValidTermsBucketWithAggs).SetName("JsonDeserialize_WithValidTermsBucketWithAggs_DeserializedCorrectly"),
        };

        private static TermsBucket ValidTermsBucketWithAggs
        {
            get
            {
                var bucket = new TermsBucket()
                {
                    DocCount = 502,
                    Key = "IT",
                };
                bucket.Add("2", new ValueAggregate() { Value = 644.0861 });
                return bucket;
            }
        }

        [TestCaseSource(nameof(FieldCapsTestCases))]
        public void TestTermsBucketAggsConverter(string queryString, object expected)
        {
            ((TermsBucket)expected).AssertJson(queryString);
        }
    }
}
