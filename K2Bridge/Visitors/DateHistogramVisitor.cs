// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2Bridge.Visitors
{
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

            dateHistogramAggregation.KustoQL =
                $"{dateHistogramAggregation.Metric} by {dateHistogramAggregation.Key} = ";

            var interval = dateHistogramAggregation.FixedInterval ?? dateHistogramAggregation.CalendarInterval;

            if (!string.IsNullOrEmpty(interval))
            {
                // https://www.elastic.co/guide/en/elasticsearch/reference/master/search-aggregations-bucket-datehistogram-aggregation.html#calendar_and_fixed_intervals
                // The interval value can get its value from two options: calendar_interval or fixed_interval.
                // If its calendar_interval, it can contain complete words like 'year', 'month' etc, so we need to check for that explicitly.
                // We also check if its a known character, if not, just use the value in the bin as-is.
                var period = interval[^1];
                dateHistogramAggregation.KustoQL += period switch
                {
                    'w' => $"{KustoQLOperators.StartOfWeek}({dateHistogramAggregation.Field})",
                    'M' => $"{KustoQLOperators.StartOfMonth}({dateHistogramAggregation.Field})",
                    'y' => $"{KustoQLOperators.StartOfYear}({dateHistogramAggregation.Field})",
                    _ when interval.Contains("week", System.StringComparison.OrdinalIgnoreCase) => $"{KustoQLOperators.StartOfWeek}({dateHistogramAggregation.Key})",
                    _ when interval.Contains("month", System.StringComparison.OrdinalIgnoreCase) => $"{KustoQLOperators.StartOfMonth}({dateHistogramAggregation.Key})",
                    _ when interval.Contains("year", System.StringComparison.OrdinalIgnoreCase) => $"{KustoQLOperators.StartOfYear}({dateHistogramAggregation.Key})",
                    _ => $"bin({dateHistogramAggregation.Field}, {interval})",
                };
            }
            else
            {
                dateHistogramAggregation.KustoQL += dateHistogramAggregation.Field;
            }

            // todatetime is redundent but we'll keep it for now
            dateHistogramAggregation.KustoQL += $"{KustoQLOperators.CommandSeparator}{KustoQLOperators.OrderBy} {dateHistogramAggregation.Key} asc";
        }
    }
}
