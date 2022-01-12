// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2Bridge.Visitors
{
    using System.Text;
    using K2Bridge.Models.Request.Aggregations;
    using K2Bridge.Models.Response;
    using K2Bridge.Utils;

    /// <content>
    /// A visitor for the <see cref="TermsAggregation"/> element.
    /// </content>
    internal partial class ElasticSearchDSLVisitor : IVisitor
    {
        /// <inheritdoc/>
        public void Visit(TermsAggregation termsAggregation)
        {
            Ensure.IsNotNull(termsAggregation, nameof(TermsAggregation));
            EnsureClause.StringIsNotNullOrEmpty(termsAggregation.Metric, nameof(TermsAggregation.Metric));
            EnsureClause.StringIsNotNullOrEmpty(termsAggregation.Field, nameof(TermsAggregation.Field));

            var subAggregations = termsAggregation.Parent.SubAggregations;

            // Extend expression: ['2']=['Carrier']
            var extendExpression = $"{EncodeKustoField(termsAggregation.Key)}={EncodeKustoField(termsAggregation.Field, true)}";

            // Bucket expression: count() by ['2']=['Carrier'] | order by count_ desc | limit 5
            var bucketExpression = new StringBuilder();
            bucketExpression.Append($"{termsAggregation.Metric} by {EncodeKustoField(termsAggregation.Key)}={EncodeKustoField(termsAggregation.Field, true)} ");

            var orderBy = termsAggregation.Order?.SortField switch
            {
                "_key" => $"{KustoQLOperators.CommandSeparator} {KustoQLOperators.OrderBy} {EncodeKustoField(termsAggregation.Key)} {termsAggregation.Order.SortOrder}",
                "_count" => $"{KustoQLOperators.CommandSeparator} {KustoQLOperators.OrderBy} {BucketColumnNames.Count} {termsAggregation.Order.SortOrder}",
                { } s => $"{KustoQLOperators.CommandSeparator} {KustoQLOperators.OrderBy} {EncodeKustoField(s)} {termsAggregation.Order.SortOrder}",
                _ => string.Empty,
            };

            bucketExpression.Append(orderBy);
            bucketExpression.Append($" {KustoQLOperators.CommandSeparator} {KustoQLOperators.Limit} {termsAggregation.Size}");

            // Build final query using termsAggregation expressions
            // let _extdata = _data | extend ['2']=['Carrier'];
            // let _summarizablemetrics = _extdata | summarize  count() by ['2']=['Carrier'] | order by count_ desc | limit 5;
            var query = BuildBucketQuery(subAggregations, extendExpression, bucketExpression.ToString());

            termsAggregation.KustoQL = query;
        }
    }
}
