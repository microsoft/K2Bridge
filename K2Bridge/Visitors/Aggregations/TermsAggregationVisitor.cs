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

            var query = new StringBuilder();

            // Add main aggregation query (summarize)
            // KQL ==> _data | summarize ['key1']=metric(field1), ['key2']=metric(field2), count() by ['key']=field
            query.Append($"({KustoTableNames.Data} | {KustoQLOperators.Summarize} {termsAggregation.SubAggregationsKustoQL}{termsAggregation.Metric} by {EncodeKustoField(termsAggregation.Key)} = {EncodeKustoField(termsAggregation.Field, true)}");

            var orderBy = termsAggregation.Order?.SortField switch
            {
                "_key" => $"{KustoQLOperators.CommandSeparator}{KustoQLOperators.OrderBy} {EncodeKustoField(termsAggregation.Key)} {termsAggregation.Order.SortOrder}",
                "_count" => $"{KustoQLOperators.CommandSeparator}{KustoQLOperators.OrderBy} {BucketColumnNames.Count} {termsAggregation.Order.SortOrder}",
                { } s => $"{KustoQLOperators.CommandSeparator}{KustoQLOperators.OrderBy} {EncodeKustoField(s)} {termsAggregation.Order.SortOrder}",
                _ => string.Empty,
            };
            query.Append(orderBy);

            // Add limit
            query.Append($"{KustoQLOperators.CommandSeparator}{KustoQLOperators.Limit} {termsAggregation.Size} | {KustoQLOperators.As} {KustoTableNames.Aggregation});");

            termsAggregation.KustoQL = query.ToString();
        }
    }
}
