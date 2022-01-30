// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

using K2Bridge.JsonConverters;
using Newtonsoft.Json;

namespace K2Bridge.Models.Response.Aggregations.Bucket;

/// <summary>
/// histogram bucket response.
/// </summary>
[JsonConverter(typeof(HistogramBucketConverter))]
public class HistogramBucket : KeyedBucket
{
    /// <summary>
    /// Gets or sets the Keyed value.
    /// </summary>
    public bool Keyed { get; set; }
}
