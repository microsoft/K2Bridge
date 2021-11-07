// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2Bridge.Visitors
{
    using K2Bridge.Models.Request.Aggregations;

    /// <content>
    /// A visitor for the different <see cref="MetricAggregation"/> types.
    /// </content>
    internal partial class ElasticSearchDSLVisitor : IVisitor
    {
        private const string ExceptionMessage = "Average FieldName must have a valid value";

        /// <inheritdoc/>
        public void Visit(AvgAggregation avgAggregation)
        {
            Ensure.IsNotNull(avgAggregation, nameof(avgAggregation));
            EnsureClause.StringIsNotNullOrEmpty(avgAggregation.FieldName, avgAggregation.FieldName, ExceptionMessage);

            avgAggregation.KustoQL = $"{KustoQLOperators.Avg}({avgAggregation.FieldName})";
            avgAggregation.FieldRenameQuery = "_" + avgAggregation.Parent + "=avg_" + avgAggregation.FieldName;
        }

        /// <inheritdoc/>
        public void Visit(CardinalityAggregation cardinalityAggregation)
        {
            Ensure.IsNotNull(cardinalityAggregation, nameof(cardinalityAggregation));
            EnsureClause.StringIsNotNullOrEmpty(cardinalityAggregation.FieldName, cardinalityAggregation.FieldName, ExceptionMessage);

            cardinalityAggregation.KustoQL = $"{KustoQLOperators.DCount}({cardinalityAggregation.FieldName})";
            cardinalityAggregation.FieldRenameQuery = "_" + cardinalityAggregation.Parent + "=count_" + cardinalityAggregation.FieldName;
        }

        /// <inheritdoc/>
        public void Visit(MaxAggregation maxAggregation)
        {
            Ensure.IsNotNull(maxAggregation, nameof(maxAggregation));
            EnsureClause.StringIsNotNullOrEmpty(maxAggregation.FieldName, maxAggregation.FieldName, ExceptionMessage);

            maxAggregation.KustoQL = $"{KustoQLOperators.Max}({maxAggregation.FieldName})";
            maxAggregation.FieldRenameQuery = "_" + maxAggregation.Parent + "=max_" + maxAggregation.FieldName;
        }
    }
}
