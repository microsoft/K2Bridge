// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using K2Bridge.Visitors;
using Newtonsoft.Json;

namespace K2Bridge.Models.Request.Aggregations.Bucket;

/// <summary>
/// Bucket aggregation that creates one bucket per filter query.
/// </summary>
internal class FiltersAggregation : BucketAggregation
{
    /// <summary>
    /// Gets or sets the filters.
    /// </summary>
    [JsonProperty("filters")]
    public Dictionary<string, FiltersBoolQuery> Filters { get; set; }

    /// <inheritdoc/>
    public override void Accept(IVisitor visitor)
    {
        visitor.Visit(this);
    }
}
