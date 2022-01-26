// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2Bridge.Visitors
{
    using System.Collections.Generic;
    using System.Text;
    using K2Bridge.Models.Request.Aggregations.Bucket.DateRange;
    using K2Bridge.Models.Request.Aggregations.Bucket.Range;

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

            var expandColumn = EncodeKustoField("_range_value");

            // Extend expression:
            // >> ['2']=pack_array("range1", "range2", "range3"), ['_range_value'] = pack_array(expr1, expr2, expr3)
            // >> | mv-expand ['2'] to typeof(string), ['_range_value']
            // >> | where ['_range_value'] == true
            var extendExpression = new StringBuilder();

            var rangeNames = new List<string>();
            var rangeExpressions = new List<string>();

            foreach (var range in dateRangeAggregation.Ranges)
            {
                range.Field = dateRangeAggregation.Field;
                range.Accept(this);

                if (string.IsNullOrEmpty(range.KustoQL))
                {
                    // This is then "open" range, -infinity to +infinity
                    rangeExpressions.Add(KustoQLOperators.True);
                }
                else
                {
                    rangeExpressions.Add(range.KustoQL);
                }

                rangeNames.Add(range.BucketNameKustoQL);
            }


            extendExpression.Append($"{EncodeKustoField(dateRangeAggregation.Key)} = {KustoQLOperators.PackArray}({string.Join(',', rangeNames)}), ");
            extendExpression.Append($"{expandColumn} = {KustoQLOperators.PackArray}({string.Join(',', rangeExpressions)})");

            extendExpression.Append($"{KustoQLOperators.CommandSeparator} {KustoQLOperators.MvExpand} {EncodeKustoField(dateRangeAggregation.Key)} to typeof(string), {expandColumn}");
            extendExpression.Append($"{KustoQLOperators.CommandSeparator} {KustoQLOperators.Where} {expandColumn} == {KustoQLOperators.True}");

            // Bucket expression:
            // >> count() by ['2']
            var bucketExpression = $"{dateRangeAggregation.Metric} by {EncodeKustoField(dateRangeAggregation.Key)}";

            // OrderBy expression:
            // >> | order by ['2'] asc
            var orderByExpression = $"{KustoQLOperators.CommandSeparator} {KustoQLOperators.OrderBy} {EncodeKustoField(dateRangeAggregation.Key)} asc";

            // Build final query using dateRangeAggregation expressions
            // let _extdata = _data
            // | extend ['2'] = pack_array("range1", "range2", "range3"), ['_range_value'] = pack_array(expr1, expr2, expr3)
            // | mv-expand ['2'] to typeof(string), ['_range_value']
            // | where ['_range_value'] == true;
            // let _summarizablemetrics = _extdata
            // | summarize count() by ['2']
            // | order by ['2'] asc;"
            var definition = new BucketAggregationQueryDefinition()
            {
                ExtendExpression = extendExpression.ToString(),
                BucketExpression = bucketExpression,
                OrderByExpression = orderByExpression,
            };

            var query = BuildBucketAggregationQuery(dateRangeAggregation, definition);

            dateRangeAggregation.KustoQL = query;
        }
    }
}
