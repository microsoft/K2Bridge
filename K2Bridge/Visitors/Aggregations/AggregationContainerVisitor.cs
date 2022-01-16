// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2Bridge.Visitors
{
    using System.Collections.Generic;
    using System.Text;
    using K2Bridge.Models.Request;
    using K2Bridge.Models.Request.Aggregations;
    using K2Bridge.Utils;

    /// <content>
    /// A visitor for the root <see cref="AggregationContainer"/> element.
    /// </content>
    internal partial class ElasticSearchDSLVisitor : IVisitor
    {
        /// <inheritdoc/>
        public void Visit(AggregationContainer aggregationContainer)
        {
            Ensure.IsNotNull(aggregationContainer, nameof(aggregationContainer));

            if (aggregationContainer.PrimaryAggregation == null)
            {
                return;
            }

            if (aggregationContainer.PrimaryAggregation is BucketAggregation bucketAggregation)
            {
                bucketAggregation.SummarizableMetricsKustoQL = BuildSummarizableMetricsQuery(
                    aggregationContainer.SubAggregations);

                bucketAggregation.PartitionableMetricsKustoQL = BuildPartitionableMetricsQuery(
                    aggregationContainer.SubAggregations,
                    bucketAggregation.Key);
            }

            aggregationContainer.PrimaryAggregation.Accept(this);
            aggregationContainer.KustoQL = aggregationContainer.PrimaryAggregation.KustoQL;
        }

        public string BuildSummarizableMetricsQuery(AggregationDictionary aggregationDictionary, string bucketMetricKey = AggregationsConstants.CountKey)
        {
            var query = new StringBuilder();

            SubQueriesStack.Add(AggregationsSubQueries.SummarizableMetricsQuery);
            VisitedMetrics.Add(bucketMetricKey);

            // Collect all ISummarizable metrics
            // ['2']=max(AvgTicketPrice), ['3']=avg(DistanceKilometers)
            var summarizableMetrics = new List<string>();
            foreach (var (_, aggregationContainer) in aggregationDictionary)
            {
                var aggregation = aggregationContainer.PrimaryAggregation;
                if (aggregation is ISummarizable)
                {
                    aggregation.Accept(this);
                    summarizableMetrics.Add($"{aggregation.KustoQL}");
                }
            }

            var summarizableMetricsExpression = string.Join(',', summarizableMetrics);
            query.Append($"{summarizableMetricsExpression}");

            if (!string.IsNullOrEmpty(summarizableMetricsExpression))
            {
                query.Append($",");
            }

            return query.ToString();
        }

        public string BuildPartitionableMetricsQuery(AggregationDictionary aggregationDictionary, string partitionKey)
        {
            // Collect all additional queries built from IPartitionable metrics
            var query = new StringBuilder();
            foreach (var (_, aggregationContainer) in aggregationDictionary)
            {
                var aggregation = aggregationContainer.PrimaryAggregation;
                if (aggregation is IPartitionable)
                {
                    ((IPartitionable)aggregation).PartitionKey = partitionKey;

                    aggregation.Accept(this);
                    query.Append($"{aggregation.KustoQL}");
                }
            }

            return query.ToString();
        }
    }
}
