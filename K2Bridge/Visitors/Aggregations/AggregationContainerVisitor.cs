// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2Bridge.Visitors;

using System.Collections.Generic;
using System.Linq;
using System.Text;
using K2Bridge.Models.Request.Aggregations;
using K2Bridge.Models.Request.Aggregations.Bucket;
using K2Bridge.Models.Request.Aggregations.Metric;

/// <content>
/// A visitor for the root <see cref="AggregationContainer"/> element.
/// </content>
internal partial class ElasticSearchDSLVisitor : IVisitor
{
    /// <inheritdoc/>
    public void Visit(AggregationContainer aggregationContainer)
    {
        Ensure.IsNotNull(aggregationContainer, nameof(aggregationContainer));

        if (aggregationContainer.PrimaryAggregation == null)
        {
            return;
        }

        if (aggregationContainer.PrimaryAggregation is BucketAggregation bucketAggregation)
        {
            VisitedMetrics.Add(EncodeKustoField(bucketAggregation.Key));
            VisitedMetrics.Add(EncodeKustoField(bucketAggregation.MetricKey));

            var primaryAggregations = aggregationContainer.SubAggregations.Values.Select(x => x.PrimaryAggregation).ToList();

            bucketAggregation.SummarizableMetricsKustoQL = BuildSummarizableMetricsQuery(
                primaryAggregations);

            bucketAggregation.PartitionableMetricsKustoQL = BuildPartitionableMetricsQuery(
                primaryAggregations,
                bucketAggregation.Key);
        }

        aggregationContainer.PrimaryAggregation.Accept(this);
        aggregationContainer.KustoQL = aggregationContainer.PrimaryAggregation.KustoQL;
    }

    public string BuildSummarizableMetricsQuery(IEnumerable<Aggregation> primaryAggregations)
    {
        var query = new StringBuilder();
        var aggregations = primaryAggregations.OfType<SummarizableMetricAggregation>();

        // Collect all SummarizableMetricAggregation metrics
        // ['2']=max(AvgTicketPrice), ['3']=avg(DistanceKilometers)
        var summarizableMetrics = new List<string>();
        foreach (var aggregation in aggregations)
        {
            aggregation.Accept(this);
            summarizableMetrics.Add($"{aggregation.KustoQL}");
        }

        var summarizableMetricsExpression = string.Join(',', summarizableMetrics);
        query.Append($"{summarizableMetricsExpression}");

        if (!string.IsNullOrEmpty(summarizableMetricsExpression))
        {
            query.Append(',');
        }

        return query.ToString();
    }

    private string BuildPartitionableMetricsQuery(IEnumerable<Aggregation> primaryAggregations, string partitionKey)
    {
        // Collect all additional queries built from PartitionableMetricAggregation metrics
        var query = new StringBuilder();
        var aggregations = primaryAggregations.OfType<PartitionableMetricAggregation>();

        foreach (var aggregation in aggregations)
        {
            aggregation.PartitionKey = partitionKey;

            aggregation.Accept(this);
            query.Append($"{aggregation.KustoQL}");
        }

        return query.ToString();
    }
}
