﻿// Copyright (c) Microsoft Corporation. All rights reserved.
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

            avgAggregation.KustoQL = $"['{avgAggregation.FieldAlias}']={KustoQLOperators.Avg}({avgAggregation.FieldName})";
        }

        /// <inheritdoc/>
        public void Visit(CardinalityAggregation cardinalityAggregation)
        {
            Ensure.IsNotNull(cardinalityAggregation, nameof(cardinalityAggregation));
            EnsureClause.StringIsNotNullOrEmpty(cardinalityAggregation.FieldName, cardinalityAggregation.FieldName, ExceptionMessage);

            cardinalityAggregation.KustoQL = $"['{cardinalityAggregation.FieldAlias}']={KustoQLOperators.DCount}({cardinalityAggregation.FieldName})";
        }

        /// <inheritdoc/>
        public void Visit(MaxAggregation maxAggregation)
        {
            Ensure.IsNotNull(maxAggregation, nameof(maxAggregation));
            EnsureClause.StringIsNotNullOrEmpty(maxAggregation.FieldName, maxAggregation.FieldName, ExceptionMessage);

            maxAggregation.KustoQL = $"['{maxAggregation.FieldAlias}']={KustoQLOperators.Max}({maxAggregation.FieldName})";
        }

        /// <inheritdoc/>
        public void Visit(SumAggregation sumAggregation)
        {
            Ensure.IsNotNull(sumAggregation, nameof(sumAggregation));
            EnsureClause.StringIsNotNullOrEmpty(sumAggregation.FieldName, sumAggregation.FieldName, ExceptionMessage);

            sumAggregation.KustoQL = $"['{sumAggregation.FieldAlias}']={KustoQLOperators.Sum}({sumAggregation.FieldName})";
        }
    }
}
