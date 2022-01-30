// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2Bridge.Visitors;

using System.Text;
using K2Bridge.Models.Request.Aggregations.Bucket;
using K2Bridge.Visitors.Aggregations.Helpers;

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

        // Extend expression: ['10'] = startofmonth(['timestamp'])
        var extendExpression = new StringBuilder();
        extendExpression.Append($"{EncodeKustoField(dateHistogramAggregation.Key)} = ");

        var interval = dateHistogramAggregation.FixedInterval ?? dateHistogramAggregation.CalendarInterval;
        var field = EncodeKustoField(dateHistogramAggregation.Field, true);
        if (!string.IsNullOrEmpty(interval))
        {
            // https://www.elastic.co/guide/en/elasticsearch/reference/master/search-aggregations-bucket-datehistogram-aggregation.html#calendar_and_fixed_intervals
            // The interval value can get its value from two options: calendar_interval or fixed_interval.
            // If its calendar_interval, it can contain complete words like 'year', 'month' etc, so we need to check for that explicitly.
            // We also check if its a known character, if not, just use the value in the bin as-is.
            var period = interval[^1];
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
            extendExpression.Append(groupExpression);
        }
        else
        {
            extendExpression.Append(field);
        }

        // Bucket expression: count() by ['10']
        var bucketExpression = $"{dateHistogramAggregation.Metric} by {EncodeKustoField(dateHistogramAggregation.Key)}";

        // OrderBy expression: | order by ['10'] asc
        var orderByExpression = $"{KustoQLOperators.CommandSeparator} {KustoQLOperators.OrderBy} {EncodeKustoField(dateHistogramAggregation.Key)} asc";

        // Build final query using dateHistogramAggregation expressions
        // let _extdata = _data
        // | extend ['10'] = startofmonth(['timestamp']);
        // let _summarizablemetrics = _extdata
        // | summarize count() by ['10']
        // | order by ['10'] asc;
        var definition = new BucketAggregationQueryDefinition()
        {
            ExtendExpression = extendExpression.ToString(),
            BucketExpression = bucketExpression,
            OrderByExpression = orderByExpression,
        };

        var query = BuildBucketAggregationQuery(dateHistogramAggregation, definition);

        dateHistogramAggregation.KustoQL = query;
    }
}
