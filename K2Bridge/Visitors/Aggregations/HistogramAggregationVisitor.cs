// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2Bridge.Visitors
{
    using System;
    using K2Bridge.Models.Request.Aggregations;
    using K2Bridge.Models.Response;
    using K2Bridge.Utils;

    /// <content>
    /// A visitor for the <see cref="HistogramAggregation"/> element.
    /// Sample KustoQL Query:
    /// (_data
    /// | where ['AvgTicketPrice'] between (50 .. 150)
    /// | summarize count() by ['key%False'] = bin(['AvgTicketPrice'], 20)
    /// | where ['count_'] >= 0
    /// | order by ['key%False'] asc ) as aggs;
    /// </content>
    internal partial class ElasticSearchDSLVisitor : IVisitor
    {
        /// <inheritdoc/>
        public void Visit(HistogramAggregation histogramAggregation)
        {
            Ensure.IsNotNull(histogramAggregation, nameof(histogramAggregation));
            EnsureClause.StringIsNotNullOrEmpty(histogramAggregation.Metric, nameof(histogramAggregation.Metric));
            EnsureClause.StringIsNotNullOrEmpty(histogramAggregation.Field, nameof(histogramAggregation.Field));

            var histogramKey = EncodeKustoField($"{histogramAggregation.Key}{AggregationsConstants.MetadataSeparator}{histogramAggregation.Keyed}");

            histogramAggregation.KustoQL = $"({KustoTableNames.Data} ";

            if (histogramAggregation.HardBounds != null)
            {
                var min = Convert.ToInt32(histogramAggregation.HardBounds.Min);
                var max = Convert.ToInt32(histogramAggregation.HardBounds.Max);
                histogramAggregation.KustoQL += $"{KustoQLOperators.CommandSeparator}{KustoQLOperators.Where} {EncodeKustoField(histogramAggregation.Field)} between ({min} .. {max})";
            }

            histogramAggregation.KustoQL += $"{KustoQLOperators.CommandSeparator}{KustoQLOperators.Summarize} {histogramAggregation.SubAggregationsKustoQL}" +
            $"{histogramAggregation.Metric} by {histogramKey} = ";

            var interval = Convert.ToInt32(histogramAggregation.Interval);
            var field = EncodeKustoField(histogramAggregation.Field, true);

            histogramAggregation.KustoQL += $"bin({field}, {interval})";
            histogramAggregation.KustoQL += $"{KustoQLOperators.CommandSeparator}{KustoQLOperators.Where} {EncodeKustoField(BucketColumnNames.Count)} >= {histogramAggregation.MinimumDocumentCount}";
            histogramAggregation.KustoQL += $"{KustoQLOperators.CommandSeparator}{KustoQLOperators.OrderBy} {histogramKey} asc) as {KustoTableNames.Aggregation};";
        }
    }
}
