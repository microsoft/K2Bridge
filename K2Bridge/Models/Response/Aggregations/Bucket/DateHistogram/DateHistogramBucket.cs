// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2Bridge.Models.Response.Aggregations
{
    using Newtonsoft.Json;
    using K2Bridge.JsonConverters;

    /// <summary>
    /// Describes date histogram bucket response element.
    /// Key is a 64 bit number representing a timestamp in milliseconds-since-the-epoch.
    /// </summary>
    [JsonConverter(typeof(DateHistogramBucketConverter))]
    public class DateHistogramBucket : KeyedBucket
    {
    }
}