// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2Bridge.Visitors
{
    using System.Globalization;
    using K2Bridge.Models.Request.Aggregations;
    using K2Bridge.Models.Response;

    /// <content>
    /// A visitor for the <see cref="RangeAggregation"/> element.
    /// </content>
    internal partial class ElasticSearchDSLVisitor : IVisitor
    {
        /// <inheritdoc/>
        public void Visit(DateRangeAggregation dateRangeAggregation)
        {
            Ensure.IsNotNull(dateRangeAggregation, nameof(RangeAggregation));
            EnsureClause.StringIsNotNullOrEmpty(dateRangeAggregation.Metric, nameof(RangeAggregation.Metric));
            EnsureClause.StringIsNotNullOrEmpty(dateRangeAggregation.Field, nameof(RangeAggregation.Field));

            // Part 1:
            // _data | extend ['_range'] = pack_array("range1", "range2", "range3"), ['_range_value']=pack_array(expr1, expr2, expr3)

            // Start of query, until first pack_array()
            dateRangeAggregation.KustoQL += $"_data | {KustoQLOperators.Extend} {EncodeKustoField(dateRangeAggregation.Key)} = {KustoQLOperators.PackArray}(";

            // Insert range names
            foreach (var range in dateRangeAggregation.Ranges)
            {
                range.Field = dateRangeAggregation.Field;
                range.Accept(this);

                dateRangeAggregation.KustoQL += $"{range.BucketNameKustoQL},";
            }

            // Remove final comma
            dateRangeAggregation.KustoQL = dateRangeAggregation.KustoQL.TrimEnd(',');

            // Close the first pack_array() and start the second pack_array()
            dateRangeAggregation.KustoQL += $"), ['_range_value'] = {KustoQLOperators.PackArray}(";

            // Insert range expressions
            foreach (var range in dateRangeAggregation.Ranges)
            {
                if (string.IsNullOrEmpty(range.KustoQL))
                {
                    // This is then "open" range, -infinity to +infinity
                    dateRangeAggregation.KustoQL += "true,";
                }
                else
                {
                    dateRangeAggregation.KustoQL += $"{range.KustoQL},";
                }
            }

            // Remove final comma
            dateRangeAggregation.KustoQL = dateRangeAggregation.KustoQL.TrimEnd(',');

            // End part 1
            dateRangeAggregation.KustoQL += ")";

            // Part 2 is expansion and filtering of rows
            dateRangeAggregation.KustoQL += $" | {KustoQLOperators.MvExpand} {EncodeKustoField(dateRangeAggregation.Key)} to typeof(string), ['_range_value']";
            dateRangeAggregation.KustoQL += $" | {KustoQLOperators.Where} ['_range_value'] == true";

            // Part 3 is the summarize part for metrics
            dateRangeAggregation.KustoQL += $" | {KustoQLOperators.Summarize} {dateRangeAggregation.SubAggregationsKustoQL}{dateRangeAggregation.Metric} by {EncodeKustoField(dateRangeAggregation.Key)}";

            // Order rows by key, and re-order columns by ascending order
            // Make sure the aggregation column (with range names) is first
            dateRangeAggregation.KustoQL += $" | {KustoQLOperators.OrderBy} {EncodeKustoField(dateRangeAggregation.Key)} asc | {KustoQLOperators.ProjectReorder} {EncodeKustoField(dateRangeAggregation.Key)}, * asc";
        }
    }
}
