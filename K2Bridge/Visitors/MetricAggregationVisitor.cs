// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2Bridge.Visitors
{
    using System.Linq;
    using System.Collections.Generic;
    using K2Bridge.Models.Request.Aggregations;

    /// <content>
    /// A visitor for the different <see cref="MetricAggregation"/> types.
    /// </content>
    internal partial class ElasticSearchDSLVisitor : IVisitor
    {
        private const string ExceptionMessage = "Average Field must have a valid value";

        /// <inheritdoc/>
        public void Visit(AverageAggregation avgAggregation)
        {
            Ensure.IsNotNull(avgAggregation, nameof(avgAggregation));
            EnsureClause.StringIsNotNullOrEmpty(avgAggregation.Field, avgAggregation.Field, ExceptionMessage);

            avgAggregation.KustoQL = $"['{avgAggregation.Key}']={KustoQLOperators.Avg}({avgAggregation.Field})";
        }

        /// <inheritdoc/>
        public void Visit(CardinalityAggregation cardinalityAggregation)
        {
            Ensure.IsNotNull(cardinalityAggregation, nameof(cardinalityAggregation));
            EnsureClause.StringIsNotNullOrEmpty(cardinalityAggregation.Field, cardinalityAggregation.Field, ExceptionMessage);

            cardinalityAggregation.KustoQL = $"['{cardinalityAggregation.Key}']={KustoQLOperators.DCount}({cardinalityAggregation.Field})";
        }

        /// <inheritdoc/>
        public void Visit(MinAggregation minAggregation)
        {
            Ensure.IsNotNull(minAggregation, nameof(minAggregation));
            EnsureClause.StringIsNotNullOrEmpty(minAggregation.Field, minAggregation.Field, ExceptionMessage);

            minAggregation.KustoQL = $"['{minAggregation.Key}']={KustoQLOperators.Min}({minAggregation.Field})";
        }

        /// <inheritdoc/>
        public void Visit(MaxAggregation maxAggregation)
        {
            Ensure.IsNotNull(maxAggregation, nameof(maxAggregation));
            EnsureClause.StringIsNotNullOrEmpty(maxAggregation.Field, maxAggregation.Field, ExceptionMessage);

            maxAggregation.KustoQL = $"['{maxAggregation.Key}']={KustoQLOperators.Max}({maxAggregation.Field})";
        }

        /// <inheritdoc/>
        public void Visit(SumAggregation sumAggregation)
        {
            Ensure.IsNotNull(sumAggregation, nameof(sumAggregation));
            EnsureClause.StringIsNotNullOrEmpty(sumAggregation.Field, sumAggregation.Field, ExceptionMessage);

            sumAggregation.KustoQL = $"['{sumAggregation.Key}']={KustoQLOperators.Sum}({sumAggregation.Field})";
        }

        /// <inheritdoc/>
        public void Visit(PercentileAggregation percentileAggregation)
        {
            Ensure.IsNotNull(percentileAggregation, nameof(percentileAggregation));
            EnsureClause.StringIsNotNullOrEmpty(percentileAggregation.Field, percentileAggregation.Field, ExceptionMessage);

            /// Median is Percentile(fieldName, 50)
            percentileAggregation.KustoQL = string.Empty;

            var valuesForColumnNames = string.Join('%', percentileAggregation.Percents.ToList().Select(item => string.Format("{0:0.0}", item)));
            var valuesForOperator = string.Join(',', percentileAggregation.Percents);

            // Example: ['A%percentile%25.0%50.0%99.0%False']=percentiles_array(fieldA, 25,50,99)']
            percentileAggregation.KustoQL += $"['{percentileAggregation.Key}%percentile%{valuesForColumnNames}%{percentileAggregation.Keyed}']={KustoQLOperators.PercentilesArray}({percentileAggregation.Field}, {valuesForOperator})";
        }
    }
}
