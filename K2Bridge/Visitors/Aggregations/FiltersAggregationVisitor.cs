// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2Bridge.Visitors;

using System.Collections.Generic;
using System.Text;
using K2Bridge.Models.Request.Aggregations.Bucket;
using K2Bridge.Utils;
using K2Bridge.Visitors.Aggregations.Helpers;

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

        var filterNames = new List<string>();
        var filterExpressions = new List<string>();

        foreach (var (key, value) in filtersAggregation.Filters)
        {
            filterNames.Add($"'{key.EscapeSlashesAndQuotes()}'");

            value.BoolQuery.Accept(this);
            filterExpressions.Add(value.BoolQuery.KustoQL);
        }

        extendExpression.Append($"{EncodeKustoField(filtersAggregation.Key)} = {KustoQLOperators.PackArray}({string.Join(',', filterNames)}), ");
        extendExpression.Append($"{expandColumn} = {KustoQLOperators.PackArray}({string.Join(',', filterExpressions)})");

        extendExpression.Append($"{KustoQLOperators.CommandSeparator} {KustoQLOperators.MvExpand} {EncodeKustoField(filtersAggregation.Key)} to typeof(string), {expandColumn}");
        extendExpression.Append($"{KustoQLOperators.CommandSeparator} {KustoQLOperators.Where} {expandColumn} == {KustoQLOperators.True}");

        // Bucket expression:
        // >> count() by ['2']
        var bucketExpression = $"{filtersAggregation.Metric} by {EncodeKustoField(filtersAggregation.Key)}";

        // OrderBy expression: | order by ['2'] asc
        var orderByExpression = $"{KustoQLOperators.CommandSeparator} {KustoQLOperators.OrderBy} {EncodeKustoField(filtersAggregation.Key)} asc";

        // Build final query using filtersAggregation expressions
        // let _extdata = _data
        // | extend ['2'] = pack_array('k1'), ['_filter_value'] = pack_array(expr1)
        // | mv-expand ['2'] to typeof(string), ['_filter_value']
        // | where ['_filter_value'] == true;
        // let _summarizablemetrics = _extdata
        // | summarize count() by ['2']
        // | order by ['2'] asc;"
        // datatable(key:string, value:string) ['2','k1'] | as metadata;
        var definition = new BucketAggregationQueryDefinition()
        {
            ExtendExpression = extendExpression.ToString(),
            BucketExpression = bucketExpression,
            OrderByExpression = orderByExpression,
            Metadata = new Dictionary<string, List<string>> {
                    { filtersAggregation.Key, filterNames },
                },
        };

        var query = BuildBucketAggregationQuery(filtersAggregation, definition);

        filtersAggregation.KustoQL = query;
    }
}
