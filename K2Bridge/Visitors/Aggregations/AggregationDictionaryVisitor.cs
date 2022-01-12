// Copyright (c) Microsoft Corporation. All rights reserved.
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
        internal string DefaultKey { get; set; } = Guid.NewGuid().ToString();

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
                // This is not a bucket aggregation scenario
                var extendExpression = $"{EncodeKustoField(DefaultKey)}={KustoQLOperators.True}";
                var bucketExpression = $"count() by {EncodeKustoField(DefaultKey)}";

                query.Append(BuildBucketQuery(aggregationDictionary, extendExpression, bucketExpression));

                // We project away the default key column if there are only ISummmarizable metrics
                if (SubQueriesStack.Last().Equals(AggregationsSubQueries.SummarizableMetricsQuery))
                {
                    projectAwayExpression = $" {KustoQLOperators.CommandSeparator} {KustoQLOperators.ProjectAway} {EncodeKustoField(DefaultKey)}";
                }
            }

            var lastQuery = SubQueriesStack.Last();
            query.Append($"{KustoQLOperators.NewLine}({lastQuery}{projectAwayExpression} {KustoQLOperators.CommandSeparator} as {KustoTableNames.Aggregations});");

            aggregationDictionary.KustoQL = query.ToString();
        }

        public string BuildBucketQuery(AggregationDictionary aggregationDictionary, string extendExpression, string bucketExpression, string bucketMetricKey = AggregationsColumns.Count)
        {
            Ensure.IsNotNull(aggregationDictionary, nameof(aggregationDictionary));

            var query = new StringBuilder();

            var extendDataQuery = BuildExtendDataQuery(aggregationDictionary, extendExpression);
            query.Append(extendDataQuery);

            var summarizableMetricsQuery = BuildSummarizableMetricsQuery(aggregationDictionary, bucketExpression, bucketMetricKey);
            query.Append(summarizableMetricsQuery);

            var nonSummarizableMetricsQuery = BuildNonSummarizableMetricsQuery(aggregationDictionary);
            query.Append(nonSummarizableMetricsQuery);

            return query.ToString();
        }

        public string BuildExtendDataQuery(AggregationDictionary aggregationDictionary, string extendExpression)
        {
            var letExtData = AggregationsSubQueries.ExtDataQuery;
            SubQueriesStack.Add(letExtData);

            var query = $"{KustoQLOperators.NewLine}{KustoQLOperators.Let} {letExtData} = {KustoTableNames.Data} {KustoQLOperators.CommandSeparator} {KustoQLOperators.Extend} {extendExpression};";

            return query;
        }

        public string BuildSummarizableMetricsQuery(AggregationDictionary aggregationDictionary, string bucketExpression, string bucketMetricKey = AggregationsColumns.Count)
        {
            var letSummarizableMetrics = AggregationsSubQueries.SummarizableMetricsQuery;
            SubQueriesStack.Add(letSummarizableMetrics);

            ParsedMetricAggregationKeys.Add(bucketMetricKey);

            // Collect all ISummarizable metrics
            // ['2']=max(AvgTicketPrice), ['3']=avg(DistanceKilometers)
            var summarizableMetrics = new List<string>();
            foreach (var (_, aggregationContainer) in aggregationDictionary)
            {
                var aggregation = aggregationContainer.PrimaryAggregation;
                if (aggregation is MetricAggregation && aggregation is ISummarizable)
                {
                    aggregation.Accept(this);
                    summarizableMetrics.Add($"{aggregation.KustoQL}");

                    ParsedMetricAggregationKeys.Add(aggregation.Key);
                }
            }

            var query = new StringBuilder();
            var summarizableMetricsExpression = string.Join(',', summarizableMetrics);

            // Build summarizable metrics query
            // let _summarizablemetrics = _extdata | summarize ['2']=max(AvgTicketPrice), ['3']=avg(DistanceKilometers)
            query.Append($"{KustoQLOperators.NewLine}{KustoQLOperators.Let} {letSummarizableMetrics} = {AggregationsSubQueries.ExtDataQuery} ");
            query.Append($"{KustoQLOperators.CommandSeparator} {KustoQLOperators.Summarize} {summarizableMetricsExpression},");

            // Query completed with bucketExpression
            query.Append($"{bucketExpression};");

            return query.ToString();
        }

        public string BuildNonSummarizableMetricsQuery(AggregationDictionary aggregationDictionary)
        {
            var query = new StringBuilder();
            foreach (var (_, aggregationContainer) in aggregationDictionary)
            {
                var aggregation = aggregationContainer.PrimaryAggregation;
                if (aggregation is MetricAggregation && aggregation is not ISummarizable)
                {
                    aggregation.Accept(this);
                    query.Append($"{aggregation.KustoQL}");

                    ParsedMetricAggregationKeys.Add(aggregation.Key);
                }
            }

            return query.ToString();
        }

        public string BuildPartitionQuery(AggregationDictionary aggregationDictionary, string bucketKey, string letQueryName, string aggregationExpression, string projectExpression, string summarizeExpression)
        {
            var query = new StringBuilder();
            var key = bucketKey ?? DefaultKey;

            string joinVariable = SubQueriesStack.Last();
            SubQueriesStack.Add(letQueryName);

            // let _tophits = _extdata
            // | join kind=inner _summarizablemetrics on ['1']
            // | partition by ['1'] (
            query.Append($"{KustoQLOperators.NewLine}{KustoQLOperators.Let} {letQueryName} = {AggregationsSubQueries.ExtDataQuery}");
            query.Append($"{KustoQLOperators.CommandSeparator} {KustoQLOperators.JoinInner} {joinVariable} on {EncodeKustoField(key)}");
            query.Append($"{KustoQLOperators.CommandSeparator} {KustoQLOperators.PartitionBy} {EncodeKustoField(key)} (");

            // Query completed with aggregationExpression
            // top 2 by timestamp asc
            query.Append($"{aggregationExpression}");

            // Project all parsed metrics with encoded keys and add projectExpression
            // | project ['2'], ['3'], ['count_'], ['4']=pack('field', AvgTicketPrice, 'order', timestamp)
            var encodedKeys = GetParsedMetricsEncodedKeys();
            if (string.IsNullOrWhiteSpace(encodedKeys))
            {
                query.Append($"{KustoQLOperators.CommandSeparator} {KustoQLOperators.Project} {projectExpression}");
            }
            else
            {
                query.Append($"{KustoQLOperators.CommandSeparator} {KustoQLOperators.Project} {encodedKeys}, {projectExpression}");
            }

            // Summarize all parsed metrics with encoded keys and add summarizeExpression
            // | summarize take_any(['2']), take_any(['3']), take_any(['count_']), ['4']=make_list(['4'])
            var encodedKeysTakeAny = GetParsedMetricsEncodedKeysTakeAny();
            if (string.IsNullOrWhiteSpace(encodedKeysTakeAny))
            {
                query.Append($"{KustoQLOperators.CommandSeparator} {KustoQLOperators.Summarize} {summarizeExpression}");
            }
            else
            {
                query.Append($"{KustoQLOperators.CommandSeparator} {KustoQLOperators.Summarize} {encodedKeysTakeAny}, {summarizeExpression}");
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
