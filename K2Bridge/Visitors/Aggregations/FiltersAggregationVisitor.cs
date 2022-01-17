// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2Bridge.Visitors
{
    using System.Collections.Generic;
    using System;
    using System.Text;
    using K2Bridge.Models.Request.Aggregations;
    using K2Bridge.Utils;

    /// <content>
    /// A visitor for the <see cref="FiltersAggregation"/> element.
    /// </content>
    internal partial class ElasticSearchDSLVisitor : IVisitor
    {
        /// <inheritdoc/>
        public void Visit(FiltersAggregation filtersAggregation)
        {
            Ensure.IsNotNull(filtersAggregation, nameof(FiltersAggregation));

            var expandColumn = EncodeKustoField("_filter_value");

            // Extend expression:
            // >> ['2']=pack_array('k1', 'k2', 'k3'), ['_filter_value']=pack_array(expr1, expr2, expr3)
            // >> | mv-expand ['2'] to typeof(string), ['_filter_value']
            // >> | where ['_filter_value'] == true
            var extendExpression = new StringBuilder();

            var metadata = new List<string>();
            var filterNames = new List<string>();
            var filterExpressions = new List<string>();

            // Assemble a column name that includes the key, and the different filters, base64-encoded
            // This name will be used in the query, instead of filtersAggregation.Key
            metadata.Add(filtersAggregation.Key);

            foreach (var (key, value) in filtersAggregation.Filters)
            {
                metadata.Add(Convert.ToBase64String(Encoding.Default.GetBytes(key)).Replace('=', '-'));

                filterNames.Add($"'{key}'");

                value.BoolQuery.Accept(this);
                filterExpressions.Add(value.BoolQuery.KustoQL);
            }

            var keyWithMetadata = string.Join(AggregationsConstants.MetadataSeparator, metadata);

            extendExpression.Append($"{EncodeKustoField(keyWithMetadata)} = {KustoQLOperators.PackArray}({string.Join(',', filterNames)}), ");
            extendExpression.Append($"{expandColumn} = {KustoQLOperators.PackArray}({string.Join(',', filterExpressions)})");

            extendExpression.Append($"{KustoQLOperators.CommandSeparator} {KustoQLOperators.MvExpand} {EncodeKustoField(keyWithMetadata)} to typeof(string), {expandColumn}");
            extendExpression.Append($"{KustoQLOperators.CommandSeparator} {KustoQLOperators.Where} {expandColumn} == {KustoQLOperators.True}");

            // Bucket expression:
            // >> count() by ['2'] | order by ['2'] asc
            var bucketExpression = new StringBuilder();

            bucketExpression.Append($"{filtersAggregation.Metric} by {EncodeKustoField(keyWithMetadata)}");
            bucketExpression.Append($"{KustoQLOperators.CommandSeparator} {KustoQLOperators.OrderBy} {EncodeKustoField(keyWithMetadata)} asc");

            // Build final query using filtersAggregation expressions
            // let _extdata = _data
            // | extend ['2'] = pack_array('k1'), ['_filter_value'] = pack_array(expr1)
            // | mv-expand ['2'] to typeof(string), ['_filter_value']
            // | where ['_filter_value'] == true;
            // let _summarizablemetrics = _extdata
            // | summarize count() by ['2']
            // | order by ['2'] asc;"
            var definition = new BucketAggregationQueryDefinition()
            {
                ExtendExpression = extendExpression.ToString(),
                BucketExpression = bucketExpression.ToString(),
            };

            var query = BuildBucketAggregationQuery(filtersAggregation, definition);

            filtersAggregation.KustoQL = query;
        }
    }
}
