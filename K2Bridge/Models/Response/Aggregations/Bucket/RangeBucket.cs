// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

using K2Bridge.JsonConverters;
using Newtonsoft.Json;

namespace K2Bridge.Models.Response.Aggregations.Bucket;

/// <summary>
/// Describes range bucket response element.
/// </summary>
[JsonConverter(typeof(RangeBucketConverter))]
public class RangeBucket : KeyedBucket
{
    /// <summary>
    /// Gets or sets the From value.
    /// </summary>
    public double? From { get; set; }

    /// <summary>
    /// Gets or sets the FromAsString value.
    /// </summary>
    public string FromAsString { get; set; }

    /// <summary>
    /// Gets or sets the To value.
    /// </summary>
    public double? To { get; set; }

    /// <summary>
    /// Gets or sets the ToAsString value.
    /// </summary>
    public string ToAsString { get; set; }
}
