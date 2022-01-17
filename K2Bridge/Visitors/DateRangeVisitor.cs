// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2Bridge.Visitors
{
    using System.Globalization;
    using System.Text;
    using K2Bridge.Models.Request.Aggregations;
    using K2Bridge.Models.Response;
    using K2Bridge.Utils;

    /// <content>
    /// A visitor for the <see cref="DateRangeAggregation"/> element.
    /// </content>
    internal partial class ElasticSearchDSLVisitor : IVisitor
    {
        /// <inheritdoc/>
        public void Visit(DateRangeAggregation dateRangeAggregation)
        {
            Ensure.IsNotNull(dateRangeAggregation, nameof(RangeAggregation));
            EnsureClause.StringIsNotNullOrEmpty(dateRangeAggregation.Metric, nameof(RangeAggregation.Metric));
            EnsureClause.StringIsNotNullOrEmpty(dateRangeAggregation.Field, nameof(RangeAggregation.Field));

            string expandColumn = EncodeKustoField("_range_value");

            var queryStringBuilder = new StringBuilder();

            // Part 1:
            // _data | extend ['_range'] = pack_array("range1", "range2", "range3"), ['_range_value']=pack_array(expr1, expr2, expr3)

            // Start of query, until first pack_array()
            queryStringBuilder.Append($"{KustoTableNames.Data} | {KustoQLOperators.Extend} {EncodeKustoField(dateRangeAggregation.Key)} = {KustoQLOperators.PackArray}(");

            // Insert range names
            foreach (var range in dateRangeAggregation.Ranges)
            {
                range.Field = dateRangeAggregation.Field;
                range.Accept(this);

                queryStringBuilder.Append($"{range.BucketNameKustoQL},");
            }

            // Remove final comma
            queryStringBuilder.Remove(queryStringBuilder.Length - 1, 1);

            // Close the first pack_array() and start the second pack_array()
            queryStringBuilder.Append($"), {expandColumn} = {KustoQLOperators.PackArray}(");

            // Insert range expressions
            foreach (var range in dateRangeAggregation.Ranges)
            {
                if (string.IsNullOrEmpty(range.KustoQL))
                {
                    // This is then "open" range, -infinity to +infinity
                    queryStringBuilder.Append("true,");
                }
                else
                {
                    queryStringBuilder.Append($"{range.KustoQL},");
                }
            }

            // Remove final comma
            queryStringBuilder.Remove(queryStringBuilder.Length - 1, 1);

            // End part 1
            queryStringBuilder.Append(")");

            // Part 2 is expansion and filtering of rows
            queryStringBuilder.Append($" | {KustoQLOperators.MvExpand} {EncodeKustoField(dateRangeAggregation.Key)} to typeof(string), {expandColumn}");
            queryStringBuilder.Append($" | {KustoQLOperators.Where} {expandColumn} == true");

            // Part 3 is the summarize part for metrics
            queryStringBuilder.Append($" | {KustoQLOperators.Summarize} {dateRangeAggregation.SubAggregationsKustoQL}{dateRangeAggregation.Metric} by {EncodeKustoField(dateRangeAggregation.Key)}");

            // Order rows by key
            queryStringBuilder.Append($" | {KustoQLOperators.OrderBy} {EncodeKustoField(dateRangeAggregation.Key)} asc");

            dateRangeAggregation.KustoQL = queryStringBuilder.ToString();
        }
    }
}
