// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2Bridge.Visitors
{
    using System.Text;
    using K2Bridge.Models.Request.Aggregations;
    using K2Bridge.Utils;

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

            var query = new StringBuilder();

            // Add main aggregation query (summarize)
            // KQL ==> _data | summarize ['key1']=metric(field1), ['key2']=metric(field2), count() by ['key']=
            query.Append($"({KustoTableNames.Data} | {KustoQLOperators.Summarize} {dateHistogramAggregation.SubAggregationsKustoQL}{dateHistogramAggregation.Metric} ");
            query.Append($"by {EncodeKustoField(dateHistogramAggregation.Key)} = ");

            // Add group expression
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
                query.Append(groupExpression);
            }
            else
            {
                query.Append(dateHistogramAggregation.Field);
            }

            // Add order by
            query.Append($"{KustoQLOperators.CommandSeparator}{KustoQLOperators.OrderBy} {EncodeKustoField(dateHistogramAggregation.Key)} asc | {KustoQLOperators.As} {KustoTableNames.Aggregation})");
            dateHistogramAggregation.KustoQL = query.ToString();
        }
    }
}
