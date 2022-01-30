// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

using K2Bridge.Models.Response.Aggregations;
using K2Bridge.Models.Response.Aggregations.Bucket;
using NUnit.Framework;

namespace K2Bridge.Tests.UnitTests.JsonConverters;

[TestFixture]
public class DateHistogramBucketConverterTests
{
    private const string ExpectedValidBucket = @"{
            ""doc_count"": 4133,
            ""key"": 1625176800000,
            ""key_as_string"": ""2021-07-02T00:00:00.000+02:00""
        }";

    private const string ExpectedValidBucketWithAggs = @"{
            ""doc_count"": 4133,
            ""key"": 1625176800000,
            ""key_as_string"": ""2021-07-02T00:00:00.000+02:00"",
            ""2"" : {""value"": 644.0861}
        }";

    private static readonly DateHistogramBucket ValidDateHistogramBucket = new()
    {
        DocCount = 4133,
        Key = 1625176800000,
        KeyAsString = "2021-07-02T00:00:00.000+02:00",
    };

    private static readonly object[] FieldCapsTestCases = {
            new TestCaseData(ExpectedValidBucket, ValidDateHistogramBucket).SetName("JsonDeserialize_WithValidDateHistogramBucket_DeserializedCorrectly"),
            new TestCaseData(ExpectedValidBucketWithAggs, ValidDateHistogramBucketWithAggs).SetName("JsonDeserialize_WithValidDateHistogramBucketWithAggs_DeserializedCorrectly"),
        };

    private static DateHistogramBucket ValidDateHistogramBucketWithAggs
    {
        get
        {
            var bucket = new DateHistogramBucket()
            {
                DocCount = 4133,
                Key = 1625176800000,
                KeyAsString = "2021-07-02T00:00:00.000+02:00",
            };
            bucket.Add("2", new ValueAggregate() { Value = 644.0861 });
            return bucket;
        }
    }

    [TestCaseSource(nameof(FieldCapsTestCases))]
    public void TestTermsBucketAggsConverter(string queryString, object expected)
    {
        ((DateHistogramBucket)expected).AssertJson(queryString);
    }
}
