// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2Bridge.Models.Request.Aggregations
{
    using K2Bridge.Visitors;
    using Newtonsoft.Json;

    /// <summary>
    /// A single-value metrics aggregation that provides an interval of plus/minus two standard deviations from the mean
    /// </summary>
    internal class ExtendedStatsAggregation : MetricAggregation, ISummarizable
    {
        /// <summary>
        /// Gets or sets Sigma field
        /// </summary>
        [JsonProperty("sigma")]
        public double Sigma { get; set; } = 2;

        /// <inheritdoc/>
        public override void Accept(IVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
}
