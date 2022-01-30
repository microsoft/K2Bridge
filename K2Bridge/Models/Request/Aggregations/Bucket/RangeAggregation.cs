// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using K2Bridge.Visitors;
using Newtonsoft.Json;

namespace K2Bridge.Models.Request.Aggregations.Bucket;

/// <summary>
/// Bucket aggregation that creates one bucket per value range of a field.
/// </summary>
internal class RangeAggregation : BucketAggregation
{
    /// <summary>
    /// Gets or sets the field to aggregate.
    /// </summary>
    [JsonProperty("field")]
    public string Field { get; set; }

    /// <summary>
    /// Gets or sets the range expressions.
    /// </summary>
    [JsonProperty("ranges")]
    public IList<RangeAggregationExpression> Ranges { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the bucket should be returned as a hash.
    /// </summary>
    [JsonProperty("keyed")]
    public bool Keyed { get; set; }

    /// <inheritdoc/>
    public override void Accept(IVisitor visitor)
    {
        visitor.Visit(this);
    }
}
