// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2Bridge.Visitors
{
    using System.Collections.Generic;
    using System.Text;
    using K2Bridge.Models.Request.Aggregations;

    /// <content>
    /// A visitor for the <see cref="RangeAggregation"/> element.
    /// </content>
    internal partial class ElasticSearchDSLVisitor : IVisitor
    {
        /// <inheritdoc/>
        public void Visit(RangeAggregation rangeAggregation)
        {
            Ensure.IsNotNull(rangeAggregation, nameof(RangeAggregation));
            EnsureClause.StringIsNotNullOrEmpty(rangeAggregation.Metric, nameof(RangeAggregation.Metric));
            EnsureClause.StringIsNotNullOrEmpty(rangeAggregation.Field, nameof(RangeAggregation.Field));

            string expandColumn = EncodeKustoField("_range_value");

            // Extend expression:
            // >> ['2']=pack_array("range1", "range2", "range3"), ['_range_value']=pack_array(expr1, expr2, expr3)
            // >> | mv-expand ['2'] to typeof(string), ['_range_value']
            // >> | where ['_range_value'] == true
            var extendExpression = new StringBuilder();

            var rangeNames = new List<string>();
            var rangeExpressions = new List<string>();

            foreach (var range in rangeAggregation.Ranges)
            {
                range.Field = rangeAggregation.Field;
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

            extendExpression.Append($"{EncodeKustoField(rangeAggregation.Key)} = {KustoQLOperators.PackArray}({string.Join(',', rangeNames)}), ");
            extendExpression.Append($"{expandColumn} = {KustoQLOperators.PackArray}({string.Join(',', rangeExpressions)}) ");

            extendExpression.Append($"{KustoQLOperators.CommandSeparator} {KustoQLOperators.MvExpand} {EncodeKustoField(rangeAggregation.Key)} to typeof(string), {expandColumn} ");
            extendExpression.Append($"{KustoQLOperators.CommandSeparator} {KustoQLOperators.Where} {expandColumn} == {KustoQLOperators.True}");

            // Bucket expression:
            // >> count() by ['2'] | order by ['2'] asc
            var bucketExpression = new StringBuilder();

            bucketExpression.Append($"{rangeAggregation.Metric} by {EncodeKustoField(rangeAggregation.Key)} ");
            bucketExpression.Append($"{KustoQLOperators.CommandSeparator} {KustoQLOperators.OrderBy} {EncodeKustoField(rangeAggregation.Key)} asc");

            // Build final query using dateRangeAggregation expressions
            var definition = new BucketAggregationQueryDefinition()
            {
                ExtendExpression = extendExpression.ToString(),
                BucketExpression = bucketExpression.ToString(),
            };

            var query = BuildBucketAggregationQuery(rangeAggregation, definition);

            rangeAggregation.KustoQL = query;
        }
    }
}
