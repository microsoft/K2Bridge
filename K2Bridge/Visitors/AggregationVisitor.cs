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

            // TODO: do something with the sub aggregations to KQL
            if (aggregationContainer.SubAggregations != null)
            {
                foreach (var (_, subAgg) in aggregationContainer.SubAggregations)
                {
                    subAgg.Accept(this);
                    if (aggregationContainer.PrimaryAggregation.GetType().BaseType.Name == "BucketAggregation")
                    {
                        // Bucket aggregations are responsible for inserting the metrics expressions in the final query
                        ((BucketAggregation)aggregationContainer.PrimaryAggregation).MetricsKustoQL += $"{subAgg.KustoQL}, ";
                    }
                    else
                    {
                        // Metrics aggregations just return their KustoQL
                        aggregationContainer.KustoQL += $"{subAgg.KustoQL}, "; // this won't work when 2+ bucket aggregations are used!
                    }
                }
            }

            aggregationContainer.PrimaryAggregation.Accept(this);

            aggregationContainer.KustoQL = aggregationContainer.PrimaryAggregation.KustoQL;
        }
    }
}
