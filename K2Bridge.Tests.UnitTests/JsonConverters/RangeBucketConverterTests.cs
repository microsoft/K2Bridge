// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace UnitTests.K2Bridge.JsonConverters
{
    using global::K2Bridge.Models.Response.Aggregations;
    using NUnit.Framework;

    [TestFixture]
    public class RangeBucketConverterTests
    {
        private const string ExpectedValidBucketFromToJSON = @"{
            ""doc_count"": 502,
            ""key"": ""foo"",
            ""from"": 0.0,
            ""to"": 800.0
        }";

        private const string ExpectedValidBucketFromJSON = @"{
            ""doc_count"": 502,
            ""key"": ""foo"",
            ""from"": 0.0
        }";

        private const string ExpectedValidBucketToJSON = @"{
            ""doc_count"": 502,
            ""key"": ""foo"",
            ""to"": 800.0
        }";

        private const string ExpectedValidBucketFromToNullKeyJSON = @"{
            ""doc_count"": 502,
            ""from"": 0.0,
            ""to"": 800.0
        }";

        private const string ExpectedValidBucketFromNullKeyJSON = @"{
            ""doc_count"": 502,
            ""from"": 0.0
        }";

        private const string ExpectedValidBucketToNullKeyJSON = @"{
            ""doc_count"": 502,
            ""to"": 800.0
        }";

        [TestCase(0, 800, "foo", ExpectedValidBucketFromToJSON)]
        [TestCase(null, 800, "foo", ExpectedValidBucketToJSON)]
        [TestCase(0, null, "foo", ExpectedValidBucketFromJSON)]
        [TestCase(0, 800, null, ExpectedValidBucketFromToNullKeyJSON)]
        [TestCase(null, 800, null, ExpectedValidBucketToNullKeyJSON)]
        [TestCase(0, null, null, ExpectedValidBucketFromNullKeyJSON)]
        public void TestRangeBucketAggsConverter(double? from, double? to, string key, string expectedJSON)
        {
            var validRangeBucket = new RangeBucket()
            {
                From = from,
                To = to,
                DocCount = 502,
                Key = key,
            };

            ((RangeBucket)validRangeBucket).AssertJson(expectedJSON);
        }
    }
}
