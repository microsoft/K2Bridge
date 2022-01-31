// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2Bridge.Models.Response.Aggregations.Bucket;

using K2Bridge.JsonConverters;
using Newtonsoft.Json;

/// <summary>
/// histogram bucket response.
/// </summary>
[JsonConverter(typeof(HistogramBucketConverter))]
public class HistogramBucket : KeyedBucket
{
    /// <summary>
    /// Gets or sets a value indicating whether this is a Keyed bucket.
    /// </summary>
    public bool Keyed { get; set; }
}
