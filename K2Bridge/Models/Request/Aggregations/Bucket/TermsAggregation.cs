// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

using K2Bridge.JsonConverters;
using K2Bridge.Visitors;
using Newtonsoft.Json;

namespace K2Bridge.Models.Request.Aggregations.Bucket;

/// <summary>
/// Bucket aggregation that creates one bucket per unique value in the designated field.
/// Default values are taken from Elasticserch API documentation.
/// </summary>
internal class TermsAggregation : BucketAggregation
{
    /// <summary>
    /// Gets or sets the field to aggregate.
    /// </summary>
    [JsonProperty("field")]
    public string Field { get; set; }

    /// <summary>
    /// Gets or sets the order of the results.
    /// </summary>
    [JsonProperty("order")]
    [JsonConverter(typeof(TermsOrderConverter))]
    public TermsOrder Order { get; set; }

    /// <summary>
    /// Gets or sets the number of buckets to return.
    /// </summary>
    [JsonProperty("size")]
    public int Size { get; set; } = 10;

    /// <inheritdoc/>
    public override void Accept(IVisitor visitor)
    {
        visitor.Visit(this);
    }
}
