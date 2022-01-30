// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2Bridge.Visitors;

using System.Text;
using K2Bridge.Models.Request.Aggregations.Bucket;
using K2Bridge.Models.Response.Aggregations;
using K2Bridge.Visitors.Aggregations.Helpers;

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

        // Extend expression: ['2']=['Carrier']
        var extendExpression = $"{EncodeKustoField(termsAggregation.Key)} = {EncodeKustoField(termsAggregation.Field, true)}";

        // Bucket expression: count() by ['2']=['Carrier'] | order by count_ desc | limit 5
        var bucketExpression = new StringBuilder();
        bucketExpression.Append($"{termsAggregation.Metric} by {EncodeKustoField(termsAggregation.Key)}");

        // OrderBy expression: | order by count_ desc
        var orderByExpression = termsAggregation.Order?.SortField switch
        {
            "_key" => $"{KustoQLOperators.CommandSeparator} {KustoQLOperators.OrderBy} {EncodeKustoField(termsAggregation.Key)} {termsAggregation.Order.SortOrder}",
            "_count" => $"{KustoQLOperators.CommandSeparator} {KustoQLOperators.OrderBy} {EncodeKustoField(BucketColumnNames.Count)} {termsAggregation.Order.SortOrder}",
            { } s => $"{KustoQLOperators.CommandSeparator} {KustoQLOperators.OrderBy} {EncodeKustoField(s)} {termsAggregation.Order.SortOrder}",
            _ => string.Empty,
        };

        // Limit expression: | limit 5
        var limitExpression = $"{KustoQLOperators.CommandSeparator} {KustoQLOperators.Limit} {termsAggregation.Size}";

        // Build final query using termsAggregation expressions
        // let _extdata = _data
        // | extend ['10'] = ['Carrier'];
        // let _summarizablemetrics = _extdata
        // | summarize ['2']=avg(['DistanceMiles']),['11']=max(['dayOfWeek']),count() by ['10'] = ['Carrier']
        // | order by ['count_'] desc\n| limit 5;"
        var definition = new BucketAggregationQueryDefinition()
        {
            ExtendExpression = extendExpression,
            BucketExpression = bucketExpression.ToString(),
            OrderByExpression = orderByExpression,
            LimitExpression = limitExpression,
        };

        var query = BuildBucketAggregationQuery(termsAggregation, definition);

        termsAggregation.KustoQL = query;
    }
}
