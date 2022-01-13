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
    /// A visitor for the root <see cref="AggregationDictionary"/> element.
    /// </content>
    internal partial class ElasticSearchDSLVisitor : IVisitor
    {
        /// <inheritdoc/>
        public void Visit(AggregationDictionary aggregationDictionary)
        {
            Ensure.IsNotNull(aggregationDictionary, nameof(aggregationDictionary));

            var query = new StringBuilder();
            var (_, firstAggregationContainer) = aggregationDictionary.First();

            if (aggregationDictionary.Count == 1 && firstAggregationContainer.PrimaryAggregation is BucketAggregation)
            {
                // This is a bucket aggregation scenario.
                // We delegate the KQL syntax construction to the aggregation container.
                firstAggregationContainer.Accept(this);
                query.Append($"\n({firstAggregationContainer.KustoQL} | as aggs);");
            }
            else
            {
                // This is not a bucket aggregation scenario.
                // Get all metrics.
                var metrics = new List<string>();
                foreach (var (_, aggregationContainer) in aggregationDictionary)
                {
                    aggregationContainer.Accept(this);
                    metrics.Add($"{aggregationContainer.KustoQL}");
                }

                // KQL ==> (_data | summarize [key1]=metric(field1), [key2]=metric(field2) | as aggs);
                var metricsKustoQL = string.Join(',', metrics);
                query.Append($"\n(_data | {KustoQLOperators.Summarize} {metricsKustoQL} | as aggs);");
            }

            aggregationDictionary.KustoQL = query.ToString();
        }

        public string BuildBucketAggregationQuery(BucketAggregation bucketAggregation, BucketAggregationQueryDefinition definition)
        {
            Ensure.IsNotNull(bucketAggregation, nameof(bucketAggregation));

            var query = new StringBuilder();

            var extendDataQuery = BuildExtendDataQuery(definition.ExtendExpression);
            query.Append(extendDataQuery);

            var summarizableMetricsQuery = $"{bucketAggregation.SubAggregationsKustoQL} {definition.BucketExpression};";
            query.Append(summarizableMetricsQuery);

            return query.ToString();
        }

        public string BuildExtendDataQuery(string extendExpression)
        {
            var letExtData = AggregationsSubQueries.ExtDataQuery;
            var query = $"{KustoQLOperators.NewLine}{KustoQLOperators.Let} {letExtData} = {KustoTableNames.Data} {KustoQLOperators.CommandSeparator} {KustoQLOperators.Extend} {extendExpression};";

            return query;
        }
    }
}
