// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2Bridge.Visitors
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using K2Bridge.Models.Request.Aggregations;
    using K2Bridge.Utils;

    /// <content>
    /// A visitor for the root <see cref="AggregationDictionary"/> element.
    /// </content>
    internal partial class ElasticSearchDSLVisitor : IVisitor
    {
        internal List<string> VisitedMetrics { get; } = new List<string>();

        internal List<string> SubQueriesStack { get; } = new List<string>() { AggregationsSubQueries.SummarizableMetricsQuery };

        /// <inheritdoc/>
        public void Visit(AggregationDictionary aggregationDictionary)
        {
            Ensure.IsNotNull(aggregationDictionary, nameof(aggregationDictionary));

            var query = new StringBuilder();
            var projectAwayExpression = string.Empty;
            var (_, firstAggregationContainer) = aggregationDictionary.First();

            if (aggregationDictionary.Count == 1 && firstAggregationContainer.PrimaryAggregation is BucketAggregation)
            {
                // This is a bucket aggregation scenario.
                // We delegate the KQL syntax construction to the aggregation container.
                firstAggregationContainer.Accept(this);
                query.Append(firstAggregationContainer.KustoQL);
            }
            else
            {
                // This is not a bucket aggregation scenario.
                string defaultKey = Guid.NewGuid().ToString();

                var defaultAggregation = new AggregationContainer()
                {
                    PrimaryAggregation = new DefaultAggregation() { Key = defaultKey },
                    SubAggregations = aggregationDictionary,
                };

                defaultAggregation.Accept(this);
                query.Append(defaultAggregation.KustoQL);

                // We project away the default key column (<guid>)
                projectAwayExpression = $"{KustoQLOperators.CommandSeparator} {KustoQLOperators.ProjectAway} {EncodeKustoField(defaultKey)}";
            }

            // (_summarizablemetrics | as aggs)
            query.Append($"{KustoQLOperators.NewLine}({SubQueriesStack.Last()}");
            query.Append($"{projectAwayExpression}");
            query.Append($"{KustoQLOperators.CommandSeparator} as {KustoTableNames.Aggregation});");

            aggregationDictionary.KustoQL = query.ToString();
        }

        public string BuildBucketAggregationQuery(BucketAggregation bucketAggregation, BucketAggregationQueryDefinition definition)
        {
            Ensure.IsNotNull(bucketAggregation, nameof(bucketAggregation));

            var query = new StringBuilder();

            query.Append($"{KustoQLOperators.NewLine}{KustoQLOperators.Let} {AggregationsSubQueries.ExtDataQuery} = {KustoTableNames.Data}");
            query.Append($"{KustoQLOperators.CommandSeparator} {KustoQLOperators.Extend} {definition.ExtendExpression};");

            query.Append($"{KustoQLOperators.NewLine}{KustoQLOperators.Let} {AggregationsSubQueries.SummarizableMetricsQuery} = {AggregationsSubQueries.ExtDataQuery}");
            query.Append($"{KustoQLOperators.CommandSeparator} {KustoQLOperators.Summarize} {bucketAggregation.SummarizableMetricsKustoQL}");
            query.Append($"{definition.BucketExpression};");

            query.Append($"{bucketAggregation.PartitionableMetricsKustoQL}");

            return query.ToString();
        }

        public string BuildPartitionQuery(PartitionQueryDefinition definition)
        {
            var query = new StringBuilder();

            string joinVariable = SubQueriesStack.Last();
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
            query.Append($" );");

            return query.ToString();
        }

        /// <summary>
        /// Gets the list of all metrics keys already visited
        /// ['2'], ['3'], ['count_']
        /// </summary>
        public string GetVisitedMetricsEncodedKeys()
        {
            string query = null;

            if (VisitedMetrics.Count > 0)
            {
                query = string.Join(',', VisitedMetrics);
            }

            return query;
        }

        /// <summary>
        /// Gets the list of all metrics keys already visited with take_any operator
        /// take_any(['2']), take_any(['3']), take_any(['count_'])
        /// </summary>
        public string GetVisitedMetricsEncodedKeysTakeAny()
        {
            string query = null;

            if (VisitedMetrics.Count > 0)
            {
                var encodedKeys = VisitedMetrics.Select(key => $"{KustoQLOperators.TakeAny}({key})");
                query = string.Join(',', encodedKeys);
            }

            return query;
        }
    }
}
