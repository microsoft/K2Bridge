﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2Bridge.Visitors
{
    using System;
    using System.Text;
    using System.Linq;
    using System.Collections.Generic;
    using K2Bridge.Models.Request.Aggregations;
    using K2Bridge.Models.Request;
    using K2Bridge.Utils;

    /// <content>
    /// A visitor for the root <see cref="AggregationDictionary"/> element.
    /// </content>
    internal partial class ElasticSearchDSLVisitor : IVisitor
    {
        internal List<string> ParsedMetricAggregationKeys { get; } = new List<string>();

        internal List<string> SubQueriesStack { get; } = new List<string>();

        /// <inheritdoc/>
        public void Visit(AggregationDictionary aggregationDictionary)
        {
            Ensure.IsNotNull(aggregationDictionary, nameof(aggregationDictionary));

            var query = new StringBuilder();
            var projectAwayExpression = string.Empty;
            var (_, firstAggregationContainer) = aggregationDictionary.First();

            if (aggregationDictionary.Count == 1 && firstAggregationContainer.PrimaryAggregation is BucketAggregation primaryAggregation)
            {
                // This is a bucket aggregation scenario
                // We delegate the KQL syntax construction to the aggregation container
                firstAggregationContainer.Accept(this);
                query.Append($"{firstAggregationContainer.KustoQL}");
            }
            else
            {
                string defaultKey = Guid.NewGuid().ToString();

                // This is not a bucket aggregation scenario
                var defaultAggregation = new AggregationContainer()
                {
                    PrimaryAggregation = new DefaultAggregation() {Key = defaultKey},
                    SubAggregations = aggregationDictionary,
                };

                defaultAggregation.Accept(this);

                query.Append(defaultAggregation.KustoQL);

                // We project away the default key column if there are only ISummmarizable metrics
                if (SubQueriesStack.Last().Equals(AggregationsSubQueries.SummarizableMetricsQuery))
                {
                    projectAwayExpression = $" {KustoQLOperators.CommandSeparator} {KustoQLOperators.ProjectAway} {EncodeKustoField(defaultKey)}";
                }
            }

            var lastQuery = SubQueriesStack.Last();
            query.Append($"{KustoQLOperators.NewLine}({lastQuery}{projectAwayExpression} {KustoQLOperators.CommandSeparator} as {KustoTableNames.Aggregations});");

            aggregationDictionary.KustoQL = query.ToString();
        }

        public string BuildBucketAggregationQuery(BucketAggregation bucketAggregation, BucketAggregationQueryDefinition definition)
        {
            Ensure.IsNotNull(bucketAggregation, nameof(bucketAggregation));

            var query = new StringBuilder();

            var extendDataQuery = BuildExtendDataQuery(definition.ExtendExpression);
            query.Append(extendDataQuery);

            var summarizableMetricsQuery = $"{bucketAggregation.SummarizableMetricsKustoQL}{definition.BucketExpression};";
            query.Append(summarizableMetricsQuery);

            var partitionableMetricsQuery = bucketAggregation.PartitionableMetricsKustoQL;
            query.Append(partitionableMetricsQuery);

            return query.ToString();
        }

        public string BuildExtendDataQuery(string extendExpression)
        {
            var letExtData = AggregationsSubQueries.ExtDataQuery;
            var query = $"{KustoQLOperators.NewLine}{KustoQLOperators.Let} {letExtData} = {KustoTableNames.Data} {KustoQLOperators.CommandSeparator} {KustoQLOperators.Extend} {extendExpression};";

            return query;
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
            var encodedKeys = GetParsedMetricsEncodedKeys();
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
            var encodedKeysTakeAny = GetParsedMetricsEncodedKeysTakeAny();
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

        public string GetParsedMetricsEncodedKeys()
        {
            string query = null;

            if (ParsedMetricAggregationKeys.Count > 0)
            {
                var encodedKeys = ParsedMetricAggregationKeys.Select(key => EncodeKustoField(key));
                query = string.Join(',', encodedKeys);
            }

            return query;
        }

        public string GetParsedMetricsEncodedKeysTakeAny()
        {
            string query = null;

            if (ParsedMetricAggregationKeys.Count > 0)
            {
                var encodedKeys = ParsedMetricAggregationKeys.Select(key => $"{KustoQLOperators.TakeAny}({EncodeKustoField(key)})");
                query = string.Join(',', encodedKeys);
            }

            return query;
        }
    }
}
