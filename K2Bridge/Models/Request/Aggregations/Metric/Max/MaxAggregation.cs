// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2Bridge.Models.Request.Aggregations
{
    using K2Bridge.Visitors;

    /// <summary>
    /// A single-value metrics aggregation that keeps track and returns the maximum value among the numeric values extracted from the aggregated documents.
    /// </summary>
    internal class MaxAggregation : MetricAggregation
    {
        public override void Accept(IVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
}