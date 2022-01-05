// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2Bridge.Visitors
{
    using System.Linq;
    using K2Bridge.Models.Request.Aggregations;
    using K2Bridge.Utils;

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

            avgAggregation.KustoQL = $"{EncodeKustoField(avgAggregation.Key)}={KustoQLOperators.Avg}({EncodeKustoField(avgAggregation)})";
        }

        /// <inheritdoc/>
        public void Visit(CardinalityAggregation cardinalityAggregation)
        {
            Ensure.IsNotNull(cardinalityAggregation, nameof(cardinalityAggregation));
            EnsureClause.StringIsNotNullOrEmpty(cardinalityAggregation.Field, cardinalityAggregation.Field, ExceptionMessage);

            cardinalityAggregation.KustoQL = $"{EncodeKustoField(cardinalityAggregation.Key)}={KustoQLOperators.DCount}({EncodeKustoField(cardinalityAggregation)})";
        }

        /// <inheritdoc/>
        public void Visit(MinAggregation minAggregation)
        {
            Ensure.IsNotNull(minAggregation, nameof(minAggregation));
            EnsureClause.StringIsNotNullOrEmpty(minAggregation.Field, minAggregation.Field, ExceptionMessage);

            minAggregation.KustoQL = $"{EncodeKustoField(minAggregation.Key)}={KustoQLOperators.Min}({EncodeKustoField(minAggregation)})";
        }

        /// <inheritdoc/>
        public void Visit(MaxAggregation maxAggregation)
        {
            Ensure.IsNotNull(maxAggregation, nameof(maxAggregation));
            EnsureClause.StringIsNotNullOrEmpty(maxAggregation.Field, maxAggregation.Field, ExceptionMessage);

            maxAggregation.KustoQL = $"{EncodeKustoField(maxAggregation.Key)}={KustoQLOperators.Max}({EncodeKustoField(maxAggregation)})";
        }

        /// <inheritdoc/>
        public void Visit(SumAggregation sumAggregation)
        {
            Ensure.IsNotNull(sumAggregation, nameof(sumAggregation));
            EnsureClause.StringIsNotNullOrEmpty(sumAggregation.Field, sumAggregation.Field, ExceptionMessage);

            sumAggregation.KustoQL = $"{EncodeKustoField(sumAggregation.Key)}={KustoQLOperators.Sum}({EncodeKustoField(sumAggregation)})";
        }

        /// <inheritdoc/>
        public void Visit(PercentileAggregation percentileAggregation)
        {
            Ensure.IsNotNull(percentileAggregation, nameof(percentileAggregation));
            EnsureClause.StringIsNotNullOrEmpty(percentileAggregation.Field, percentileAggregation.Field, ExceptionMessage);

            var sep = AggregationsConstants.MetadataSeparator;

            var valuesForColumnNames = string.Join(sep, percentileAggregation.Percents.ToList().Select(item => $"{item:0.0}"));
            var valuesForOperator = string.Join(',', percentileAggregation.Percents);

            // We don't use EncodeKustoField on this key because it contains a '.' but isn't dynamic
            // Example: ['A%percentile%25.0%50.0%99.0%False']=percentiles_array(fieldA, 25,50,99)']
            var key = $"['{percentileAggregation.Key}{sep}percentile{sep}{valuesForColumnNames}{sep}{percentileAggregation.Keyed}']";
            percentileAggregation.KustoQL = $"{key}={KustoQLOperators.PercentilesArray}({EncodeKustoField(percentileAggregation)}, {valuesForOperator})";
        }
    }
}
