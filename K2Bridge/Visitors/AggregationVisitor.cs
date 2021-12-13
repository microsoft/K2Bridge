// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2Bridge.Visitors
{
    using K2Bridge.Models.Request.Aggregations;

    /// <content>
    /// A visitor for the root <see cref="Aggregation"/> element.
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

            // First, go over the sub-aggregations (metrics) and assemble their KustoQL snippets
            if (aggregationContainer.SubAggregations != null)
            {
                foreach (var (_, subAgg) in aggregationContainer.SubAggregations)
                {
                    // Visit the aggregation
                    subAgg.Accept(this);
                    // Bucket aggregations are responsible for inserting the metrics expressions in the final query
                    if (aggregationContainer.PrimaryAggregation is BucketAggregation bucketAggregation)
                    {
                        // Store the metrics snippet in the bucket aggregation
                        bucketAggregation.MetricsKustoQL += $"{subAgg.KustoQL}, ";
                    }
                }
            }

            // Visit the Primary aggregation
            // If it is a bucket aggregation, it will return a full query including metrics
            // If it is a metric aggregation, it will return its KustoQL snippet
            aggregationContainer.PrimaryAggregation.Accept(this);

            aggregationContainer.KustoQL = aggregationContainer.PrimaryAggregation.KustoQL;
        }
    }
}
