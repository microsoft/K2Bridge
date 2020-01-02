// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2Bridge.Visitors
{
    internal partial class ElasticSearchDSLVisitor : IVisitor
    {
        public void Visit(Models.Request.Aggregations.DateHistogramAggregation dateHistogramAggregation)
        {
            dateHistogramAggregation.KQL = $"{dateHistogramAggregation.Metric} by {dateHistogramAggregation.FieldName} = ";
            if (!string.IsNullOrEmpty(dateHistogramAggregation.Interval))
            {
                var period = dateHistogramAggregation.Interval[dateHistogramAggregation.Interval.Length - 1];
                switch (period)
                {
                    case 'w':
                        dateHistogramAggregation.KQL += $"{KQLOperators.StartOfWeek}({dateHistogramAggregation.FieldName})";
                        break;
                    case 'M':
                        dateHistogramAggregation.KQL += $"{KQLOperators.StartOfMonth}({dateHistogramAggregation.FieldName})";
                        break;
                    case 'y':
                        dateHistogramAggregation.KQL += $"{KQLOperators.StartOfYear}({dateHistogramAggregation.FieldName})";
                        break;
                    default:
                        // todatetime is redundent but we'll keep it for now
                        dateHistogramAggregation.KQL += $"bin({KQLOperators.ToDateTime}({dateHistogramAggregation.FieldName}), {dateHistogramAggregation.Interval})";
                        break;
                }
            }
            else
            {
                dateHistogramAggregation.KQL += dateHistogramAggregation.FieldName;
            }

            // todatetime is redundent but we'll keep it for now
            dateHistogramAggregation.KQL += $" | {KQLOperators.OrderBy} {KQLOperators.ToDateTime}({dateHistogramAggregation.FieldName}) asc";
        }
    }
}
