// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2Bridge.Visitors
{
    using System.Text;
    using K2Bridge.Models.Request.Aggregations;

    /// <content>
    /// A visitor for the <see cref="DateHistogramAggregation"/> element.
    /// </content>
    internal partial class ElasticSearchDSLVisitor : IVisitor
    {
        /// <inheritdoc/>
        public void Visit(DateHistogramAggregation dateHistogramAggregation)
        {
            Ensure.IsNotNull(dateHistogramAggregation, nameof(dateHistogramAggregation));
            EnsureClause.StringIsNotNullOrEmpty(dateHistogramAggregation.Metric, nameof(dateHistogramAggregation.Metric));
            EnsureClause.StringIsNotNullOrEmpty(dateHistogramAggregation.Field, nameof(dateHistogramAggregation.Field));

            // Extend expression: ['2']=['timestamp']
            var extendExpression = $"{EncodeKustoField(dateHistogramAggregation.Key)} = {EncodeKustoField(dateHistogramAggregation.Field, true)}";

            // Bucket expression: count() by ['2']=startofmonth(['timestamp'])| order by ['2'] asc
            var bucketExpression = new StringBuilder();
            bucketExpression.Append($"{dateHistogramAggregation.Metric} by {EncodeKustoField(dateHistogramAggregation.Key)} = ");

            var interval = dateHistogramAggregation.FixedInterval ?? dateHistogramAggregation.CalendarInterval;
            if (!string.IsNullOrEmpty(interval))
            {
                // https://www.elastic.co/guide/en/elasticsearch/reference/master/search-aggregations-bucket-datehistogram-aggregation.html#calendar_and_fixed_intervals
                // The interval value can get its value from two options: calendar_interval or fixed_interval.
                // If its calendar_interval, it can contain complete words like 'year', 'month' etc, so we need to check for that explicitly.
                // We also check if its a known character, if not, just use the value in the bin as-is.
                var period = interval[^1];
                var field = EncodeKustoField(dateHistogramAggregation.Field, true);
                var groupExpression = period switch
                {
                    'w' => $"{KustoQLOperators.StartOfWeek}({field})",
                    'M' => $"{KustoQLOperators.StartOfMonth}({field})",
                    'y' => $"{KustoQLOperators.StartOfYear}({field})",
                    _ when interval.Contains("week", System.StringComparison.OrdinalIgnoreCase) => $"{KustoQLOperators.StartOfWeek}({field})",
                    _ when interval.Contains("month", System.StringComparison.OrdinalIgnoreCase) => $"{KustoQLOperators.StartOfMonth}({field})",
                    _ when interval.Contains("year", System.StringComparison.OrdinalIgnoreCase) => $"{KustoQLOperators.StartOfYear}({field})",
                    _ => $"bin({field}, {interval})",
                };
                bucketExpression.Append(groupExpression);
            }
            else
            {
                bucketExpression.Append(dateHistogramAggregation.Field);
            }

            bucketExpression.Append($" {KustoQLOperators.CommandSeparator} {KustoQLOperators.OrderBy} {EncodeKustoField(dateHistogramAggregation.Key)} asc");

            // Build final query using dateHistogramAggregation expressions
            // let _extdata = _data | extend ['2']=['timestamp'];
            // let _summarizablemetrics = _extdata | summarize  count() by ['2']=startofmonth(['timestamp'])| order by ['2'] asc;
            var definition = new BucketAggregationQueryDefinition()
            {
                ExtendExpression = extendExpression,
                BucketExpression = bucketExpression.ToString(),
            };

            var query = BuildBucketAggregationQuery(dateHistogramAggregation, definition);

            dateHistogramAggregation.KustoQL = query;
        }
    }
}
