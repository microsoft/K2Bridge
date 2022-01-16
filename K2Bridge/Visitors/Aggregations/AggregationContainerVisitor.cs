// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2Bridge.Visitors
{
    using System.Collections.Generic;
    using System.Text;
    using K2Bridge.Models.Request;
    using K2Bridge.Models.Request.Aggregations;

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
                bucketAggregation.SubAggregationsKustoQL = BuildSummarizableMetricsQuery(aggregationContainer.SubAggregations);
            }

            aggregationContainer.PrimaryAggregation.Accept(this);
            aggregationContainer.KustoQL = aggregationContainer.PrimaryAggregation.KustoQL;
        }

        public string BuildSummarizableMetricsQuery(AggregationDictionary aggregationDictionary)
        {
            var query = new StringBuilder();

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
    }
}
