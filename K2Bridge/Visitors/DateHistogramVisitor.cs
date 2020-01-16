// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2Bridge.Visitors
{
    using System;
    using K2Bridge.Models.Request.Aggregations;

    internal partial class ElasticSearchDSLVisitor : IVisitor
    {
        public void Visit(DateHistogramAggregation dateHistogramAggregation)
        {
            if (dateHistogramAggregation == null)
            {
                throw new ArgumentException(
                    "Argument cannot be null",
                    nameof(dateHistogramAggregation));
            }

            if (string.IsNullOrEmpty(dateHistogramAggregation.Metric) ||
                string.IsNullOrEmpty(dateHistogramAggregation.FieldName))
            {
                throw new IllegalClauseException();
            }

            dateHistogramAggregation.KQL =
                $"{dateHistogramAggregation.Metric} by {dateHistogramAggregation.FieldName} = ";
            if (!string.IsNullOrEmpty(dateHistogramAggregation.Interval))
            {
                var period = dateHistogramAggregation.Interval[^1];
                dateHistogramAggregation.KQL += period switch
                {
                    'w' => $"{KQLOperators.StartOfWeek}({dateHistogramAggregation.FieldName})",
                    'M' => $"{KQLOperators.StartOfMonth}({dateHistogramAggregation.FieldName})",
                    'y' => $"{KQLOperators.StartOfYear}({dateHistogramAggregation.FieldName})",
                    _ => $"bin({dateHistogramAggregation.FieldName}, {dateHistogramAggregation.Interval})",
                };
            }
            else
            {
                dateHistogramAggregation.KQL += dateHistogramAggregation.FieldName;
            }

            // todatetime is redundent but we'll keep it for now
            dateHistogramAggregation.KQL += $"{KQLOperators.CommandSeparator}{KQLOperators.OrderBy} {dateHistogramAggregation.FieldName} asc";
        }
    }
}
