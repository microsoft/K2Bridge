// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2Bridge.Models.Request.Aggregations.Metric
{
    using K2Bridge.Visitors;

    /// <summary>
    /// A single-value metrics aggregation that calculates an approximate count of distinct values.
    /// </summary>
    internal class CardinalityAggregation : SummarizableMetricAggregation
    {
        /// <inheritdoc/>
        public override void Accept(IVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
}
