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
        public void Visit(Aggregation aggregation)
        {
            Ensure.IsNotNull(aggregation, nameof(aggregation));

            if (aggregation.PrimaryAggregation == null)
            {
                return;
            }

            aggregation.PrimaryAggregation.Accept(this);

            // TODO: do something with the sub aggregations to KQL
            if (aggregation.SubAggregations != null)
            {
                foreach (var (_, subAgg) in aggregation.SubAggregations)
                {
                    subAgg.Accept(this);

                    aggregation.KustoQL += $"{subAgg.KustoQL}, "; // this won't work when 2+ bucket aggregations are used!
                }
            }

            aggregation.KustoQL += aggregation.PrimaryAggregation.KustoQL;
        }
    }
}