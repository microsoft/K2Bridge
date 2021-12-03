// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2Bridge.Models.Request.Aggregations
{
    using K2Bridge.JsonConverters;
    using K2Bridge.Visitors;
    using Newtonsoft.Json;

    /// <summary>
    /// A single-value metrics aggregation that computes the median of numeric values that are extracted from the aggregated documents.
    /// </summary>
    [JsonConverter(typeof(PercentileAggregationFieldConverter))]
    internal class PercentileAggregation : MetricAggregation
    {
        /// <summary>
        /// Gets or sets field value for metric computation.
        /// </summary>
        [JsonProperty("percents")]
        public double[] Percents { get; set; }

        [JsonProperty("keyed")]
        public bool? Keyed { get; set; }

        /// <inheritdoc/>
        public override void Accept(IVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
}