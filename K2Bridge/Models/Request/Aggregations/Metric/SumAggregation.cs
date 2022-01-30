// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

using K2Bridge.Visitors;

namespace K2Bridge.Models.Request.Aggregations.Metric;

/// <summary>
/// A single-value metrics aggregation that computes the sum of numeric values that are extracted from the aggregated documents.
/// </summary>
internal class SumAggregation : SummarizableMetricAggregation
{
    /// <inheritdoc/>
    public override void Accept(IVisitor visitor)
    {
        visitor.Visit(this);
    }
}
