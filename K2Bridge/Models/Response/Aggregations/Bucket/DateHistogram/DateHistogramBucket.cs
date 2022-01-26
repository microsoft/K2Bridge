// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2Bridge.Models.Response.Aggregations.Bucket.DateHistogram
{
    using K2Bridge.JsonConverters;
    using K2Bridge.Models.Response.Aggregations.Bucket;
    using Newtonsoft.Json;

    /// <summary>
    /// Describes date histogram bucket response element.
    /// Key is a 64 bit number representing a timestamp in milliseconds-since-the-epoch.
    /// </summary>
    [JsonConverter(typeof(DateHistogramBucketConverter))]
    public class DateHistogramBucket : KeyedBucket
    {
    }
}
