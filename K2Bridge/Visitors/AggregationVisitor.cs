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

            if (aggregation.PrimaryAggregation.GetType().Name == typeof(RangeAggregation).Name)
            {
                // The range aggregation might have a KustoQL stanza to insert before the summarize
                if (!string.IsNullOrEmpty(aggregation.PrimaryAggregation.KustoQLPreSummarize))
                {
                    aggregation.KustoQL += aggregation.PrimaryAggregation.KustoQLPreSummarize;
                }
            }

            // TODO: do something with the sub aggregations to KQL
            if (aggregation.SubAggregations != null)
            {
                // If we have any sub-aggregations (metrics), insert the summarize now
                if (aggregation.PrimaryAggregation.GetType().BaseType.Name == typeof(BucketAggregation).Name)
                {
                    aggregation.KustoQL += $"{KustoQLOperators.CommandSeparator}{KustoQLOperators.Summarize} ";
                }

                // Process sub-aggregations
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
