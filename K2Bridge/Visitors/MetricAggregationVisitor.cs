// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2Bridge.Visitors
{
    using K2Bridge.Models.Request.Aggregations;

    internal partial class ElasticSearchDSLVisitor : IVisitor
    {
        private const string ExceptionMessage = "Average FieldName must have a valid value";

        public void Visit(AvgAggregation avgAggregation)
        {
            Ensure.IsNotNull(avgAggregation, nameof(avgAggregation));
            EnsureClause.StringIsNotNullOrEmpty(avgAggregation.FieldName, avgAggregation.FieldName, ExceptionMessage);

            avgAggregation.KustoQL = $"{KustoQLOperators.Avg}({avgAggregation.FieldName})";
        }

        public void Visit(CardinalityAggregation cardinalityAggregation)
        {
            Ensure.IsNotNull(cardinalityAggregation, nameof(cardinalityAggregation));
            EnsureClause.StringIsNotNullOrEmpty(cardinalityAggregation.FieldName, cardinalityAggregation.FieldName, ExceptionMessage);

            cardinalityAggregation.KustoQL = $"{KustoQLOperators.DCount}({cardinalityAggregation.FieldName})";
        }
    }
}
