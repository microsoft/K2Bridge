// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2Bridge.Visitors
{
    using System;
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
            var projectAwayExpression = string.Empty;
            var (_, firstAggregationContainer) = aggregationDictionary.First();

            if (aggregationDictionary.Count == 1 && firstAggregationContainer.PrimaryAggregation is BucketAggregation)
            {
                // This is a bucket aggregation scenario.
                // We delegate the KQL syntax construction to the aggregation container.
                firstAggregationContainer.Accept(this);
                query.Append(firstAggregationContainer.KustoQL);
            }
            else
            {
                // This is not a bucket aggregation scenario.
                string defaultKey = Guid.NewGuid().ToString();

                var defaultAggregation = new AggregationContainer()
                {
                    PrimaryAggregation = new DefaultAggregation() { Key = defaultKey },
                    SubAggregations = aggregationDictionary,
                };

                defaultAggregation.Accept(this);
                query.Append(defaultAggregation.KustoQL);

                // We project away the default key column
                projectAwayExpression = $"{KustoQLOperators.CommandSeparator} {KustoQLOperators.ProjectAway} {EncodeKustoField(defaultKey)}";
            }

            // (_summarizablemetrics | as aggs)
            query.Append($"{KustoQLOperators.NewLine}({AggregationsSubQueries.SummarizableMetricsQuery}");
            query.Append(projectAwayExpression);
            query.Append($"{KustoQLOperators.CommandSeparator} as {KustoTableNames.Aggregation});");

            aggregationDictionary.KustoQL = query.ToString();
        }

        public string BuildBucketAggregationQuery(BucketAggregation bucketAggregation, BucketAggregationQueryDefinition definition)
        {
            Ensure.IsNotNull(bucketAggregation, nameof(bucketAggregation));

            var query = new StringBuilder();

            query.Append($"{KustoQLOperators.NewLine}{KustoQLOperators.Let} {AggregationsSubQueries.ExtDataQuery} = {KustoTableNames.Data}");
            query.Append($"{KustoQLOperators.CommandSeparator} {KustoQLOperators.Extend} {definition.ExtendExpression};");

            query.Append($"{KustoQLOperators.NewLine}{KustoQLOperators.Let} {AggregationsSubQueries.SummarizableMetricsQuery} = {AggregationsSubQueries.ExtDataQuery}");
            query.Append($"{KustoQLOperators.CommandSeparator} {KustoQLOperators.Summarize} {bucketAggregation.SubAggregationsKustoQL}");
            query.Append($"{definition.BucketExpression};");

            return query.ToString();
        }
    }
}
