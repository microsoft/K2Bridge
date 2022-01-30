// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Linq;
using System.Text;
using K2Bridge.Models.Request.Aggregations.Metric;
using K2Bridge.Utils;
using K2Bridge.Visitors.Aggregations.Helpers;

namespace K2Bridge.Visitors;

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

        VisitedMetrics.Add(EncodeKustoField(avgAggregation.Key));
    }

    /// <inheritdoc/>
    public void Visit(CardinalityAggregation cardinalityAggregation)
    {
        Ensure.IsNotNull(cardinalityAggregation, nameof(cardinalityAggregation));
        EnsureClause.StringIsNotNullOrEmpty(cardinalityAggregation.Field, cardinalityAggregation.Field, ExceptionMessage);

        cardinalityAggregation.KustoQL = $"{EncodeKustoField(cardinalityAggregation.Key)}={KustoQLOperators.DCount}({EncodeKustoField(cardinalityAggregation)})";

        VisitedMetrics.Add(EncodeKustoField(cardinalityAggregation.Key));
    }

    /// <inheritdoc/>
    public void Visit(ExtendedStatsAggregation extendedStatsAggregation)
    {
        Ensure.IsNotNull(extendedStatsAggregation, nameof(extendedStatsAggregation));
        EnsureClause.StringIsNotNullOrEmpty(extendedStatsAggregation.Field, extendedStatsAggregation.Field, ExceptionMessage);

        var metadata = new List<string>
            {
                extendedStatsAggregation.Key,
                AggregationsConstants.ExtendedStats,
                extendedStatsAggregation.Sigma.ToString(),
            };

        var keyWithMetadata = EncodeKustoField(string.Join(AggregationsConstants.MetadataSeparator, metadata));

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

        query.Append($"{keyWithMetadata}={KustoQLOperators.Pack}(");
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
        query.Append(')');

        extendedStatsAggregation.KustoQL = query.ToString();

        VisitedMetrics.Add(keyWithMetadata);
    }

    /// <inheritdoc/>
    public void Visit(MinAggregation minAggregation)
    {
        Ensure.IsNotNull(minAggregation, nameof(minAggregation));
        EnsureClause.StringIsNotNullOrEmpty(minAggregation.Field, minAggregation.Field, ExceptionMessage);

        minAggregation.KustoQL = $"{EncodeKustoField(minAggregation.Key)}={KustoQLOperators.Min}({EncodeKustoField(minAggregation)})";

        VisitedMetrics.Add(EncodeKustoField(minAggregation.Key));
    }

    /// <inheritdoc/>
    public void Visit(MaxAggregation maxAggregation)
    {
        Ensure.IsNotNull(maxAggregation, nameof(maxAggregation));
        EnsureClause.StringIsNotNullOrEmpty(maxAggregation.Field, maxAggregation.Field, ExceptionMessage);

        maxAggregation.KustoQL = $"{EncodeKustoField(maxAggregation.Key)}={KustoQLOperators.Max}({EncodeKustoField(maxAggregation)})";

        VisitedMetrics.Add(EncodeKustoField(maxAggregation.Key));
    }

    /// <inheritdoc/>
    public void Visit(SumAggregation sumAggregation)
    {
        Ensure.IsNotNull(sumAggregation, nameof(sumAggregation));
        EnsureClause.StringIsNotNullOrEmpty(sumAggregation.Field, sumAggregation.Field, ExceptionMessage);

        sumAggregation.KustoQL = $"{EncodeKustoField(sumAggregation.Key)}={KustoQLOperators.Sum}({EncodeKustoField(sumAggregation)})";

        VisitedMetrics.Add(EncodeKustoField(sumAggregation.Key));
    }

    /// <inheritdoc/>
    public void Visit(PercentileAggregation percentileAggregation)
    {
        Ensure.IsNotNull(percentileAggregation, nameof(percentileAggregation));
        EnsureClause.StringIsNotNullOrEmpty(percentileAggregation.Field, percentileAggregation.Field, ExceptionMessage);

        var sep = AggregationsConstants.MetadataSeparator;

        var valuesForColumnNames = string.Join(sep, percentileAggregation.Percents.ToList().Select(item => $"{item:0.0}"));
        var valuesForOperator = string.Join(',', percentileAggregation.Percents);

        var metadata = new List<string>
            {
                percentileAggregation.Key,
                AggregationsConstants.Percentile,
                valuesForColumnNames,
                percentileAggregation.Keyed.ToString(),
            };

        // We don't use EncodeKustoField on this key because it contains a '.' but isn't dynamic
        // Example: ['A%percentile%25.0%50.0%99.0%False']=percentiles_array(fieldA, 25,50,99)']
        var keyWithMetadata = $"['{string.Join(AggregationsConstants.MetadataSeparator, metadata)}']";
        percentileAggregation.KustoQL = $"{keyWithMetadata}={KustoQLOperators.PercentilesArray}({EncodeKustoField(percentileAggregation)}, {valuesForOperator})";

        VisitedMetrics.Add(keyWithMetadata);
    }

    /// <inheritdoc/>
    public void Visit(TopHitsAggregation topHitsAggregation)
    {
        Ensure.IsNotNull(topHitsAggregation, nameof(topHitsAggregation));
        EnsureClause.StringIsNotNullOrEmpty(topHitsAggregation.Field, topHitsAggregation.Field, ExceptionMessage);

        // top 2 by timestamp asc
        var sort = topHitsAggregation.Sort.First();
        var topHitsExpression = $"{KustoQLOperators.Top} {topHitsAggregation.Size} by {EncodeKustoField(sort.FieldName, true)} {sort.Order}";

        // ['4']=pack('field', AvgTicketPrice, 'order', timestamp)
        var data = new Dictionary<string, string>
            {
                { $"'{AggregationsConstants.SourceField}'", $"'{topHitsAggregation.Field}'" },
                { $"'{AggregationsConstants.SourceValue}'", EncodeKustoField(topHitsAggregation.Field) },
                { $"'{AggregationsConstants.SortValue}'", EncodeKustoField(sort.FieldName) },
            };

        var packedData = string.Join(',', data.Select(item => $"{item.Key},{item.Value}"));
        var projectExpression = $"{EncodeKustoField(topHitsAggregation.Key)}={KustoQLOperators.Pack}({packedData})";

        // ['4']=make_list(['4'])
        var metadata = new List<string>
            {
                topHitsAggregation.Key,
                AggregationsConstants.TopHits,
            };

        var keyWithMetadata = EncodeKustoField(string.Join(AggregationsConstants.MetadataSeparator, metadata));
        var summarizeExpression = $"{keyWithMetadata}={KustoQLOperators.MakeList}({EncodeKustoField(topHitsAggregation.Key)})";

        var partionQueryName = $"_{AggregationsConstants.TopHits}{topHitsAggregation.Key}";

        var definition = new PartitionQueryDefinition()
        {
            AggregationExpression = topHitsExpression,
            ProjectExpression = projectExpression,
            SummarizeExpression = summarizeExpression,
            PartitionKey = topHitsAggregation.PartitionKey,
            PartionQueryName = partionQueryName,
        };

        var query = BuildPartitionQuery(definition);

        topHitsAggregation.KustoQL = query;

        VisitedMetrics.Add(keyWithMetadata);
    }
}
