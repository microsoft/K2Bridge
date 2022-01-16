// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2Bridge.Visitors
{
    using System.Linq;
    using System.Text;
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
        public void Visit(ExtendedStatsAggregation extendedStatsAggregation)
        {
            Ensure.IsNotNull(extendedStatsAggregation, nameof(extendedStatsAggregation));
            EnsureClause.StringIsNotNullOrEmpty(extendedStatsAggregation.Field, extendedStatsAggregation.Field, ExceptionMessage);

            var key = EncodeKustoField($"{extendedStatsAggregation.Key}{AggregationsConstants.MetadataSeparator}extended_stats{AggregationsConstants.MetadataSeparator}{extendedStatsAggregation.Sigma}");
            var count = $"{KustoQLOperators.Count}()";
            var min = $"{KustoQLOperators.Min}({EncodeKustoField(extendedStatsAggregation)})";
            var max = $"{KustoQLOperators.Max}({EncodeKustoField(extendedStatsAggregation)})";
            var avg = $"{KustoQLOperators.Avg}({EncodeKustoField(extendedStatsAggregation)})";
            var sum = $"{KustoQLOperators.Sum}({EncodeKustoField(extendedStatsAggregation)})";
            var sumOfSquares = $"{KustoQLOperators.Sum}({KustoQLOperators.Pow}({EncodeKustoField(extendedStatsAggregation)}, 2))";
            var variancePopulation = $"{KustoQLOperators.VariancePopulation}({EncodeKustoField(extendedStatsAggregation)})";
            var varianceSampling = $"{KustoQLOperators.Variance}({EncodeKustoField(extendedStatsAggregation)})";
            var standardDeviationPopulation = $"{KustoQLOperators.StDevPopulation}({EncodeKustoField(extendedStatsAggregation)})";
            var standardDeviationSampling = $"{KustoQLOperators.StDev}({EncodeKustoField(extendedStatsAggregation)})";

            var query = new StringBuilder();

            query.Append($"{key}={KustoQLOperators.Pack}(");
            query.Append($"'{AggregationsConstants.Count}', {count},");
            query.Append($"'{AggregationsConstants.Min}', {min},");
            query.Append($"'{AggregationsConstants.Max}', {max},");
            query.Append($"'{AggregationsConstants.Average}', {avg},");
            query.Append($"'{AggregationsConstants.Sum}', {sum},");
            query.Append($"'{AggregationsConstants.SumOfSquares}', {sumOfSquares},");
            query.Append($"'{AggregationsConstants.VariancePopulation}', {variancePopulation},");
            query.Append($"'{AggregationsConstants.VarianceSampling}', {varianceSampling},");
            query.Append($"'{AggregationsConstants.StandardDeviationPopulation}', {standardDeviationPopulation},");
            query.Append($"'{AggregationsConstants.StandardDeviationSampling}', {standardDeviationSampling}");
            query.Append(")");

            extendedStatsAggregation.KustoQL = query.ToString();
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

        /// <inheritdoc/>
        public void Visit(TopHitsAggregation topHitsAggregation)
        {

        }
    }
}
