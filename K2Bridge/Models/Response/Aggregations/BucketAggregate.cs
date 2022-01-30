// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using K2Bridge.JsonConverters;
using K2Bridge.Models.Response.Aggregations.Bucket;
using Newtonsoft.Json;

namespace K2Bridge.Models.Response.Aggregations;

/// <summary>
/// Describes bucket aggregate response element.
/// </summary>
[JsonConverter(typeof(BucketAggregateConverter))]
public class BucketAggregate : IAggregate
{
    /// <summary>
    /// Gets a collection of buckets.
    /// </summary>
    public List<IKeyedBucket> Buckets { get; } = new List<IKeyedBucket>();

    /// <summary>
    /// Gets or sets a value indicating whether it's a Keyed aggregate.
    /// </summary>
    public bool Keyed { get; set; }
}
