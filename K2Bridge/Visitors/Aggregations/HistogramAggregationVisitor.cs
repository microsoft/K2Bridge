﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2Bridge.Visitors
{
    using System;
    using System.Text;
    using K2Bridge.Models.Request.Aggregations;
    using K2Bridge.Models.Response;
    using K2Bridge.Utils;

    /// <content>
    /// A visitor for the <see cref="HistogramAggregation"/> element.
    /// Sample KustoQL Query:
    /// | where ['AvgTicketPrice'] >= bin(50,20) and ['AvgTicketPrice'] < bin(150,20)+20
    /// (_data | summarize count() by ['2%False'] = bin(['AvgTicketPrice'], 20)
    /// | where ['count_'] >= 1
    /// | order by ['2%False'] asc | as aggs);
    /// </content>
    internal partial class ElasticSearchDSLVisitor : IVisitor
    {
        /// <inheritdoc/>
        public void Visit(HistogramAggregation histogramAggregation)
        {
            Ensure.IsNotNull(histogramAggregation, nameof(histogramAggregation));
            EnsureClause.StringIsNotNullOrEmpty(histogramAggregation.Metric, nameof(histogramAggregation.Metric));
            EnsureClause.StringIsNotNullOrEmpty(histogramAggregation.Field, nameof(histogramAggregation.Field));

            var interval = Convert.ToInt32(histogramAggregation.Interval);
            var field = EncodeKustoField(histogramAggregation.Field, true);

            var histogramKey = EncodeKustoField($"{histogramAggregation.Key}{AggregationsConstants.MetadataSeparator}{histogramAggregation.Keyed}");

            // Extend expression:
            // | extend ['2%False'] = bin(['AvgTicketPrice'], 20)
            // | where ['AvgTicketPrice'] >= bin(50,20) and ['AvgTicketPrice'] < bin(150,20)+20;
            var extendExpression = new StringBuilder();
            extendExpression.Append($"{histogramKey} = bin({field}, {interval})");
            extendExpression.Append($"{KustoQLOperators.CommandSeparator}");

            if (histogramAggregation.HardBounds != null)
            {
                var min = Convert.ToInt32(histogramAggregation.HardBounds.Min);
                var max = Convert.ToInt32(histogramAggregation.HardBounds.Max);

                extendExpression.Append($"{KustoQLOperators.CommandSeparator} {KustoQLOperators.Where} {field} >= bin({min}, {interval}) and {field} < bin({max}, {interval})+{interval}");
            }

            // Bucket expression: count() by ['2%False'] | order by ['2%False'] asc
            var bucketExpression = new StringBuilder();
            bucketExpression.Append($"{histogramAggregation.Metric} by {histogramKey}");
            bucketExpression.Append($"{KustoQLOperators.CommandSeparator} {KustoQLOperators.OrderBy} {histogramKey} asc");

            // Build final query using dateHistogramAggregation expressions
            // let _extdata = _data
            // | extend ['2%False'] = bin(['AvgTicketPrice'], 20)
            // | where ['AvgTicketPrice'] >= bin(50,20) and ['AvgTicketPrice'] < bin(150,20)+20;
            // let _summarizablemetrics = _extdata
            // | summarize count() by ['2%False']
            // | order by ['2%False'] asc;
            var definition = new BucketAggregationQueryDefinition()
            {
                ExtendExpression = extendExpression.ToString(),
                BucketExpression = bucketExpression.ToString(),
            };

            var query = BuildBucketAggregationQuery(histogramAggregation, definition);

            histogramAggregation.KustoQL = query;
        }
    }
}
