// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2Bridge.Visitors
{
    using System;
    using K2Bridge.Models.Request.Aggregations;

    /// <content>
    /// A visitor for the <see cref="HistogramAggregation"/> element.
    /// </content>
    internal partial class ElasticSearchDSLVisitor : IVisitor
    {
        /// <inheritdoc/>
        public void Visit(HistogramAggregation histogramAggregation)
        {
            Ensure.IsNotNull(histogramAggregation, nameof(histogramAggregation));
            EnsureClause.StringIsNotNullOrEmpty(histogramAggregation.Metric, nameof(histogramAggregation.Metric));
            EnsureClause.StringIsNotNullOrEmpty(histogramAggregation.Field, nameof(histogramAggregation.Field));

            histogramAggregation.KustoQL = $"_data | {KustoQLOperators.Summarize} " + histogramAggregation.SubAggregationsKustoQL +
            $"{histogramAggregation.Metric} by {EncodeKustoField(histogramAggregation.Key + "%" + histogramAggregation.Keyed)} = ";

            var interval = Convert.ToInt32(histogramAggregation.Interval);
            var field = EncodeKustoField(histogramAggregation.Field, true);

            histogramAggregation.KustoQL += $"bin({field}, {interval})";

            // todatetime is redundent but we'll keep it for now
            histogramAggregation.KustoQL += $"{KustoQLOperators.CommandSeparator}{KustoQLOperators.OrderBy} {EncodeKustoField(histogramAggregation.Key + "%" + histogramAggregation.Keyed)} asc";
            if (histogramAggregation.HardBounds != null)
            {
                var min = Convert.ToInt32(histogramAggregation.HardBounds.Min) - interval;
                var max = Convert.ToInt32(histogramAggregation.HardBounds.Max) + interval;
                histogramAggregation.KustoQL += $" | where {EncodeKustoField(histogramAggregation.Key + "%" + histogramAggregation.Keyed)} between (" + min + " .. " + max + ") ";
            }
        }
    }
}
