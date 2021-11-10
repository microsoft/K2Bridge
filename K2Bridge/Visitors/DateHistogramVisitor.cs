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
            EnsureClause.StringIsNotNullOrEmpty(dateHistogramAggregation.FieldName, nameof(dateHistogramAggregation.FieldName));

            dateHistogramAggregation.KustoQL =
                $"{dateHistogramAggregation.Metric} by {dateHistogramAggregation.FieldName} = ";
            if (!string.IsNullOrEmpty(dateHistogramAggregation.Interval))
            {
                var period = dateHistogramAggregation.Interval[^1];
                if (dateHistogramAggregation.Interval.Contains("week", System.StringComparison.OrdinalIgnoreCase))
                {
                    dateHistogramAggregation.KustoQL += $"{KustoQLOperators.StartOfWeek}({dateHistogramAggregation.FieldName})";
                } else if (dateHistogramAggregation.Interval.Contains("month", System.StringComparison.OrdinalIgnoreCase))
                {
                    dateHistogramAggregation.KustoQL += $"{KustoQLOperators.StartOfMonth}({dateHistogramAggregation.FieldName})";
                } else if (dateHistogramAggregation.Interval.Contains("year", System.StringComparison.OrdinalIgnoreCase))
                {
                    dateHistogramAggregation.KustoQL += $"{KustoQLOperators.StartOfYear}({dateHistogramAggregation.FieldName})";
                }
                else
                {
                    dateHistogramAggregation.KustoQL += period switch
                    {
                        'w' => $"{KustoQLOperators.StartOfWeek}({dateHistogramAggregation.FieldName})",
                        'M' => $"{KustoQLOperators.StartOfMonth}({dateHistogramAggregation.FieldName})",
                        'y' => $"{KustoQLOperators.StartOfYear}({dateHistogramAggregation.FieldName})",
                        _ => $"bin({dateHistogramAggregation.FieldName}, {dateHistogramAggregation.Interval})",
                    };
                }
            }
            else
            {
                dateHistogramAggregation.KustoQL += dateHistogramAggregation.FieldName;
            }

            // todatetime is redundent but we'll keep it for now
            dateHistogramAggregation.KustoQL += $"{KustoQLOperators.CommandSeparator}{KustoQLOperators.OrderBy} {dateHistogramAggregation.FieldName} asc";
        }
    }
}
