// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2Bridge.Visitors
{
    using K2Bridge.Models.Request.Aggregations;

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
            EnsureClause.StringIsNotNullOrEmpty(termsAggregation.FieldName, nameof(TermsAggregation.FieldName));

            termsAggregation.KustoQL = $"{termsAggregation.Metric} by ['{termsAggregation.FieldAlias}'] = {termsAggregation.FieldName}";

            if (termsAggregation.SortFieldName == "_key")
            {
                // Alphabetical order
                termsAggregation.KustoQL += $"{KustoQLOperators.CommandSeparator}{KustoQLOperators.OrderBy} ['{termsAggregation.FieldAlias}'] {termsAggregation.SortOrder}";
            }
            else if (termsAggregation.SortFieldName == "_count")
            {
                // Count order
                termsAggregation.KustoQL += $"{KustoQLOperators.CommandSeparator}{KustoQLOperators.OrderBy} count_ {termsAggregation.SortOrder}";
            }
            else
            {
                // Custom order
                termsAggregation.KustoQL += $"{KustoQLOperators.CommandSeparator}{KustoQLOperators.OrderBy} ['_{termsAggregation.SortFieldName}'] {termsAggregation.SortOrder}";
            }

            termsAggregation.KustoQL += $"{KustoQLOperators.CommandSeparator}{KustoQLOperators.Limit} {termsAggregation.Size}";
        }
    }
}
