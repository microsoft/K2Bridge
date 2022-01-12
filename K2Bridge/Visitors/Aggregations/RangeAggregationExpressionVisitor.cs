// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2Bridge.Visitors
{
    using K2Bridge.Models.Request.Aggregations;

    /// <content>
    /// A visitor for the <see cref="RangeAggregationExpression"/> element.
    /// </content>
    internal partial class ElasticSearchDSLVisitor : IVisitor
    {
        /// <inheritdoc/>
        public void Visit(RangeAggregationExpression rangeExpression)
        {
            Ensure.IsNotNull(rangeExpression, nameof(rangeExpression));
            EnsureClause.StringIsNotNullOrEmpty(rangeExpression.Field, nameof(rangeExpression.Field));

            var field = EncodeKustoField(rangeExpression.Field, true);
            var gteExpr = rangeExpression.From.HasValue ? $"{field} >= {rangeExpression.From}" : null;
            var ltExpr = rangeExpression.To.HasValue ? $"{field} < {rangeExpression.To}" : null;

            rangeExpression.BucketNameKustoQL = $"{rangeExpression.From ?? '*'}-{rangeExpression.To ?? '*'}";

            rangeExpression.KustoQL = (gteExpr, ltExpr) switch
            {
                (null, null) => string.Empty,
                (null, _) => ltExpr,
                (_, null) => gteExpr,
                (_, _) => $"{gteExpr} {KustoQLOperators.And} {ltExpr}",
            };
        }
    }
}