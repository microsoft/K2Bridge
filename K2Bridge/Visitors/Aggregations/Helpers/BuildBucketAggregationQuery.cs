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
    /// Helper function to build KQL bucket aggregation.
    /// </content>
    internal partial class ElasticSearchDSLVisitor : IVisitor
    {
        public string BuildBucketAggregationQuery(BucketAggregation bucketAggregation, BucketAggregationQueryDefinition definition)
        {
            Ensure.IsNotNull(bucketAggregation, nameof(bucketAggregation));

            var query = new StringBuilder();

            query.Append($"{KustoQLOperators.NewLine}{KustoQLOperators.Let} {AggregationsSubQueries.ExtDataQuery} = {KustoTableNames.Data}");
            query.Append($"{KustoQLOperators.CommandSeparator} {KustoQLOperators.Extend} {definition.ExtendExpression};");

            query.Append($"{KustoQLOperators.NewLine}{KustoQLOperators.Let} {AggregationsSubQueries.SummarizableMetricsQuery} = {AggregationsSubQueries.ExtDataQuery}");
            query.Append($"{KustoQLOperators.CommandSeparator} {KustoQLOperators.Summarize} {bucketAggregation.SummarizableMetricsKustoQL}");
            query.Append($"{definition.BucketExpression}{definition.OrderByExpression}{definition.LimitExpression};");

            query.Append($"{bucketAggregation.PartitionableMetricsKustoQL}");

            // (_summarizablemetrics | as aggs)
            query.Append($"{KustoQLOperators.NewLine}({SubQueriesStack.Last()}");
            query.Append(definition.ProjectAwayExpression);
            query.Append(definition.OrderByExpression);
            query.Append($"{KustoQLOperators.CommandSeparator} as {KustoTableNames.Aggregation});");

            if (definition.Metadata != null)
            {
                query.Append(BuildMetadataQuery(definition.Metadata));
            }

            return query.ToString();
        }
    }
}
