// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2Bridge.Visitors;

using System.Collections.Generic;
using System.Linq;
using System.Text;
using K2Bridge.Utils;
using K2Bridge.Visitors.Aggregations.Helpers;

/// <content>
/// Helper function to build partition query.
/// </content>
internal partial class ElasticSearchDSLVisitor : IVisitor
{
    /// <summary>
    /// Gets a list of visited metrics.
    /// Each time a metric aggregation is parsed, its key is added to the list.
    /// Helps to propagate all previous aggregations results in BuildPartitionQuery.
    /// </summary>
    internal List<string> VisitedMetrics { get; } = new List<string>();

    private string BuildPartitionQuery(PartitionQueryDefinition definition)
    {
        var query = new StringBuilder();

        var joinVariable = SubQueriesStack.Last();
        SubQueriesStack.Add(definition.PartionQueryName);

        // let _tophits = _extdata
        // | join kind=inner _summarizablemetrics on ['1']
        // | partition by ['1'] (
        query.Append($"{KustoQLOperators.NewLine}{KustoQLOperators.Let} {definition.PartionQueryName} = {AggregationsSubQueries.ExtDataQuery}");
        query.Append($"{KustoQLOperators.CommandSeparator} {KustoQLOperators.JoinInner} {joinVariable} on {EncodeKustoField(definition.PartitionKey)}");
        query.Append($"{KustoQLOperators.CommandSeparator} {KustoQLOperators.PartitionBy} {EncodeKustoField(definition.PartitionKey)} (");

        // Query completed with aggregationExpression
        // top 2 by timestamp asc
        query.Append($"{definition.AggregationExpression}");

        // Project all parsed metrics with encoded keys and add projectExpression
        // | project ['2'], ['3'], ['count_'], ['4']=pack('field', AvgTicketPrice, 'order', timestamp)
        var encodedKeys = GetVisitedMetricsEncodedKeys();
        if (string.IsNullOrWhiteSpace(encodedKeys))
        {
            query.Append($"{KustoQLOperators.CommandSeparator} {KustoQLOperators.Project} {definition.ProjectExpression}");
        }
        else
        {
            query.Append($"{KustoQLOperators.CommandSeparator} {KustoQLOperators.Project} {encodedKeys}, {definition.ProjectExpression}");
        }

        // Summarize all parsed metrics with encoded keys and add summarizeExpression
        // | summarize take_any(['2']), take_any(['3']), take_any(['count_']), ['4']=make_list(['4'])
        var encodedKeysTakeAny = GetVisitedMetricsEncodedKeysTakeAny();
        if (string.IsNullOrWhiteSpace(encodedKeysTakeAny))
        {
            query.Append($"{KustoQLOperators.CommandSeparator} {KustoQLOperators.Summarize} {definition.SummarizeExpression}");
        }
        else
        {
            query.Append($"{KustoQLOperators.CommandSeparator} {KustoQLOperators.Summarize} {encodedKeysTakeAny}, {definition.SummarizeExpression}");
        }

        // We close the partition expression
        query.Append($");");

        return query.ToString();
    }

    /// <summary>
    /// Gets the list of all metrics keys already visited
    /// ['2'], ['3'], ['count_'].
    /// </summary>
    private string GetVisitedMetricsEncodedKeys()
    {
        return string.Join(',', VisitedMetrics);
    }

    /// <summary>
    /// Gets the list of all metrics keys already visited with take_any operator
    /// take_any(['2']), take_any(['3']), take_any(['count_'])
    /// </summary>
    private string GetVisitedMetricsEncodedKeysTakeAny()
    {
        var encodedKeys = VisitedMetrics.Select(key => $"{KustoQLOperators.TakeAny}({key})");
        return string.Join(',', encodedKeys);
    }
}
