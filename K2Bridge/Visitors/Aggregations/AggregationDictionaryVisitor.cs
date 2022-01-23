// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2Bridge.Visitors
{
    using System;
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
                var defaultKey = Guid.NewGuid().ToString();

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

        /// <summary>
        /// Given a metadata dictionary, generates a Kusto QL statement creating the metadata table:
        /// datatable(['key']:string, ['value']:string) ['2', 'val1', '2', 'val2', '2', 'val3'] | as metadata;
        /// The table has two columns, key and value.
        /// The key is the aggregation key, e.g. '2'.
        /// The value is an expected bucket name, e.g. 'val1'.
        /// </summary>
        /// <param name="metadata">A metadata dictionary.</param>
        /// <returns>A string containing the Kusto QL statement.</returns>
        private static string BuildMetadataQuery(Dictionary<string, List<string>> metadata)
        {
            var query = new StringBuilder();

            query.Append($"{KustoQLOperators.NewLine}{KustoQLOperators.Datatable}({KustoTableNames.MetadataKey}:{KustoTableNames.MetadataColumnType}, {KustoTableNames.MetadataValue}:{KustoTableNames.MetadataColumnType}) [");

            var values = new List<string>();

            // Keys
            foreach (var (key, valueList) in metadata)
            {
                // Values
                foreach (var value in valueList)
                {
                    values.Add($"'{key}',{value}");
                }
            }

            query.Append(string.Join(',', values));

            query.Append(']');

            query.Append($" | as {KustoTableNames.Metadata};");

            return query.ToString();
        }

        private string BuildBucketAggregationQuery(BucketAggregation bucketAggregation, BucketAggregationQueryDefinition definition)
        {
            Ensure.IsNotNull(bucketAggregation, nameof(bucketAggregation));

            var query = new StringBuilder();

            query.Append($"{KustoQLOperators.NewLine}{KustoQLOperators.Let} {AggregationsSubQueries.ExtDataQuery} = {KustoTableNames.Data}");
            query.Append($"{KustoQLOperators.CommandSeparator} {KustoQLOperators.Extend} {definition.ExtendExpression};");

            query.Append($"{KustoQLOperators.NewLine}{KustoQLOperators.Let} {AggregationsSubQueries.SummarizableMetricsQuery} = {AggregationsSubQueries.ExtDataQuery}");
            query.Append($"{KustoQLOperators.CommandSeparator} {KustoQLOperators.Summarize} {bucketAggregation.SubAggregationsKustoQL}");
            query.Append($"{definition.BucketExpression};");

            if (definition.Metadata != null)
            {
                query.Append(BuildMetadataQuery(definition.Metadata));
            }

            return query.ToString();
        }
    }
}
