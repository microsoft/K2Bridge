// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2Bridge.Visitors
{
    using K2Bridge.Models.Request.Aggregations.Bucket;

    /// <content>
    /// A visitor for the <see cref="DefaultAggregation"/> element.
    /// </content>
    internal partial class ElasticSearchDSLVisitor : IVisitor
    {
        /// <inheritdoc/>
        public void Visit(DefaultAggregation defaultAggregation)
        {
            Ensure.IsNotNull(defaultAggregation, nameof(DefaultAggregation));

            var extendExpression = $"{EncodeKustoField(defaultAggregation.Key)} = {KustoQLOperators.True}";
            var bucketExpression = $"{defaultAggregation.Metric} by {EncodeKustoField(defaultAggregation.Key)}";
            var projectAwayExpression = $"{KustoQLOperators.CommandSeparator} {KustoQLOperators.ProjectAway} {EncodeKustoField(defaultAggregation.Key)}";

            var definition = new BucketAggregationQueryDefinition()
            {
                ExtendExpression = extendExpression,
                BucketExpression = bucketExpression,
                ProjectAwayExpression = projectAwayExpression,
            };

            // Build final query using defaultAggregation expressions
            // let _extdata = _data
            // | extend [<Guid>] = True;
            // let _summarizablemetrics = _extdata
            // | summarize count() by [<Guid>];
            var query = BuildBucketAggregationQuery(defaultAggregation, definition);

            defaultAggregation.KustoQL = query;
        }
    }
}
