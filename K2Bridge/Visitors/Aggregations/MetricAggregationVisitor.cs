// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2Bridge.Visitors
{
    using System.Collections.Generic;
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

            var valuesForColumnNames = string.Join(AggregationsMetadata.Separator, percentileAggregation.Percents.ToList().Select(item => $"{item:0.0}"));
            var valuesForOperator = string.Join(',', percentileAggregation.Percents);

            var metadata = new List<string>();
            metadata.Add(percentileAggregation.Key);
            metadata.Add(AggregationsColumns.Percentile);
            metadata.Add(valuesForColumnNames);
            metadata.Add(percentileAggregation.Keyed.ToString());

            // We don't use EncodeKustoField on this key because it contains a '.' but isn't dynamic
            // Example: ['A%percentile%25.0%50.0%99.0%False']=percentiles_array(fieldA, 25,50,99)']
            var keyWithMetadata=$"['{string.Join(AggregationsMetadata.Separator, metadata)}']";
            percentileAggregation.KustoQL = $"{keyWithMetadata}={KustoQLOperators.PercentilesArray}({EncodeKustoField(percentileAggregation)}, {valuesForOperator})";
        }

        /// <inheritdoc/>
        public void Visit(TopHitsAggregation topHitsAggregation)
        {
            Ensure.IsNotNull(topHitsAggregation, nameof(topHitsAggregation));
            EnsureClause.StringIsNotNullOrEmpty(topHitsAggregation.Field, topHitsAggregation.Field, ExceptionMessage);

            var aggregationDictionary = topHitsAggregation.Parent;
            var primaryAggregation = aggregationDictionary?.Parent?.PrimaryAggregation;

            // top 2 by timestamp asc
            var sort = topHitsAggregation.Sort.First();
            var topHitsExpression = $"{KustoQLOperators.Top} {topHitsAggregation.Size} by {sort.FieldName} {sort.Order}";

            // ['4']=pack('field', AvgTicketPrice, 'order', timestamp)
            var pack = $"{KustoQLOperators.Pack}('{topHitsAggregation.Field}', {EncodeKustoField(topHitsAggregation)}, '{sort.FieldName}', {EncodeKustoField(sort.FieldName)})";
            var projectExpression = $"{EncodeKustoField(topHitsAggregation.Key)}={pack}";

            // ['4']=make_list(['4'])
            var keyWithMetadata = $"{topHitsAggregation.Key}{AggregationsMetadata.Separator}{AggregationsColumns.TopHits}";
            var summarizeExpression = $"{EncodeKustoField(keyWithMetadata)}={KustoQLOperators.MakeList}({EncodeKustoField(topHitsAggregation.Key)})";

            var subQueryName = $"_{AggregationsColumns.TopHits}{topHitsAggregation.Key}";
            var query = BuildPartitionQuery(topHitsAggregation.Parent, primaryAggregation?.Key, subQueryName, topHitsExpression, projectExpression, summarizeExpression);

            topHitsAggregation.KustoQL = query;
        }
    }
}
