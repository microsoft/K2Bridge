// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2Bridge.Visitors
{
    internal partial class ElasticSearchDSLVisitor : IVisitor
    {
        public void Visit(Models.Request.Aggregations.Aggregation aggregation)
        {
            if (aggregation.PrimaryAggregation == null)
            {
                return;
            }

            aggregation.PrimaryAggregation.Accept(this);

            // TODO: do something with the sub aggregations to KQL
            if (aggregation.SubAggregations != null)
            {
                foreach (var aggKeyPair in aggregation.SubAggregations)
                {
                    string subName = aggKeyPair.Key;
                    var subAgg = aggKeyPair.Value;
                    subAgg.Accept(this);

                    aggregation.KQL += $"{subAgg.KQL}, "; // this won't work when 2+ bucket aggregations are used!
                }
            }

            aggregation.KQL += aggregation.PrimaryAggregation.KQL;
        }
    }
}
