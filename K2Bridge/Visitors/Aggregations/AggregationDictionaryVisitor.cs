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

        internal List<string> ParsedAggregationKeys { get; } = new List<string>();

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
                var bucketKey = DefaultKey;
                var extendExpression = KustoQLOperators.True;
                var bucketExpression = $"by {EncodeKustoField(DefaultKey)}";

                query.Append(BuildBucketQuery(aggregationDictionary, bucketKey, extendExpression, bucketExpression));

                // We project away the default key column if there are only ISummmarizable metrics
                if (SubQueriesStack.Last().Equals(AggregationsConstants.SummarizableMetricsQuery))
                {
                    projectAwayExpression = $" {KustoQLOperators.CommandSeparator} {KustoQLOperators.ProjectAway} {EncodeKustoField(DefaultKey)}";
                }
            }

            var lastQuery = SubQueriesStack.Last();
            query.Append($"{KustoQLOperators.NewLine}( {lastQuery}{projectAwayExpression} {KustoQLOperators.CommandSeparator} as aggs );");

            aggregationDictionary.KustoQL = query.ToString();
        }

        public string BuildBucketQuery(AggregationDictionary aggregationDictionary, string bucketKey, string extendExpression, string bucketExpression)
        {
            var query = new StringBuilder();

            var extendDataQuery = BuildExtendDataQuery(aggregationDictionary, bucketKey, extendExpression);
            query.Append(extendDataQuery);

            var summarizableMetricsQuery = BuildSummarizableMetricsQuery(aggregationDictionary, bucketExpression);
            query.Append(summarizableMetricsQuery);

            var nonSummarizableMetricsQuery = BuildNonSummarizableMetricsQuery(aggregationDictionary);
            query.Append(nonSummarizableMetricsQuery);

            return query.ToString();
        }

        public string BuildExtendDataQuery(AggregationDictionary aggregationDictionary, string bucketKey, string extendExpression)
        {
            var letExtData = AggregationsConstants.ExtDataQuery;
            SubQueriesStack.Add(letExtData);

            var query = $"{KustoQLOperators.NewLine}{KustoQLOperators.Let} {letExtData} = _data {KustoQLOperators.CommandSeparator} {KustoQLOperators.Extend} {EncodeKustoField(bucketKey)}={extendExpression};";

            return query;
        }

        public string BuildSummarizableMetricsQuery(AggregationDictionary aggregationDictionary, string bucketExpression)
        {
            if (!ContainsSummarizableMetrics(aggregationDictionary) && !IsBucketAggregation(aggregationDictionary))
            {
                return null;
            }

            var letSummarizableMetrics = AggregationsConstants.SummarizableMetricsQuery;
            SubQueriesStack.Add(letSummarizableMetrics);

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

                    ParsedAggregationKeys.Add(aggregation.Key);
                }
            }

            var query = new StringBuilder();
            var summarizableMetricsExpression = string.Join(',', summarizableMetrics);

            // Build summarizable metrics query
            // let _summarizablemetrics = _extdata | summarize ['2']=max(AvgTicketPrice), ['3']=avg(DistanceKilometers)
            query.Append($"{KustoQLOperators.NewLine}{KustoQLOperators.Let} {letSummarizableMetrics} = _extdata {KustoQLOperators.CommandSeparator} {KustoQLOperators.Summarize} {summarizableMetricsExpression}");

            // Query completed by bucket aggregation expression
            if (!string.IsNullOrEmpty(summarizableMetricsExpression) && IsBucketAggregation(aggregationDictionary))
            {
                query.Append($",");
            }

            query.Append($" {bucketExpression};");

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

                    ParsedAggregationKeys.Add(aggregation.Key);
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
            query.Append($"{KustoQLOperators.NewLine}{KustoQLOperators.Let} {letQueryName} = _extdata");

            // If previous sub query was _extdata, table join is not needed
            if (joinVariable.Equals(AggregationsConstants.ExtDataQuery))
            {
                query.Append($"{KustoQLOperators.CommandSeparator}");
            }
            else
            {
                // | join kind=inner _summarizablemetrics on ['1']
                // | partition by ['1'] (
                query.Append($"{KustoQLOperators.CommandSeparator} {KustoQLOperators.JoinInner} {joinVariable} on {EncodeKustoField(key)}");
                query.Append($"{KustoQLOperators.CommandSeparator} {KustoQLOperators.PartitionBy} {EncodeKustoField(key)} (");
            }

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
            if (!joinVariable.Equals(AggregationsConstants.ExtDataQuery))
            {
                query.Append($" )");
            }

            query.Append(";");
            return query.ToString();
        }

        public string GetParsedMetricsEncodedKeys()
        {
            string query = null;

            if (ParsedAggregationKeys.Count > 0)
            {
                var encodedKeys = ParsedAggregationKeys.Select(key => EncodeKustoField(key));
                query = string.Join(',', encodedKeys);
            }

            return query;
        }

        public string GetParsedMetricsEncodedKeysTakeAny()
        {
            string query = null;

            if (ParsedAggregationKeys.Count > 0)
            {
                var encodedKeys = ParsedAggregationKeys.Select(key => $"{KustoQLOperators.TakeAny}({EncodeKustoField(key)})");
                query = string.Join(',', encodedKeys);
            }

            return query;
        }

        public bool IsBucketAggregation(AggregationDictionary aggregationDictionary)
        {
            Ensure.IsNotNull(aggregationDictionary, nameof(aggregationDictionary));

            return aggregationDictionary.Parent?.PrimaryAggregation is BucketAggregation;
        }

        public bool ContainsSummarizableMetrics(AggregationDictionary aggregationDictionary)
        {
            Ensure.IsNotNull(aggregationDictionary, nameof(aggregationDictionary));

            foreach (var (_, aggregationContainer) in aggregationDictionary)
            {
                var aggregation = aggregationContainer.PrimaryAggregation;
                if (aggregation is MetricAggregation && aggregation is ISummarizable)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
