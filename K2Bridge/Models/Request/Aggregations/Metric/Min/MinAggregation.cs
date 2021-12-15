// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2Bridge.Models.Request.Aggregations
{
    using K2Bridge.Visitors;

    /// <summary>
    /// A single-value metrics aggregation that computes the min of numeric values that are extracted from the aggregated documents.
    /// </summary>
    public class MinAggregation : MetricAggregation
    {
        /// <inheritdoc/>
        public override void Accept(IVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
}
