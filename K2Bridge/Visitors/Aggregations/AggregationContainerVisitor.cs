// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2Bridge.Visitors
{
    using System.Text;
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
                // Get all sub aggregation metrics
                // KQL ==> [key1]=metric(field1), [key2]=metric(field2), (will be appended with count())
                var metrics = new StringBuilder();
                if (aggregationContainer.SubAggregations?.Count > 0)
                {
                    foreach (var (_, subAgg) in aggregationContainer.SubAggregations)
                    {
                        subAgg.Accept(this);
                        metrics.Append($"{subAgg.KustoQL}, ");
                    }

                    bucketAggregation.SubAggregationsKustoQL = metrics.ToString();
                }
            }

            aggregationContainer.PrimaryAggregation.Accept(this);
            aggregationContainer.KustoQL = aggregationContainer.PrimaryAggregation.KustoQL;
        }
    }
}
