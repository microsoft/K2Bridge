// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2Bridge.Visitors
{
    using K2Bridge.Models.Request.Aggregations;
    using K2Bridge.Models.Response;

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

            termsAggregation.KustoQL = $"{termsAggregation.Metric} by ['{termsAggregation.Key}'] = {termsAggregation.Field}";

            if (termsAggregation.Order.Field == "_key")
            {
                // Alphabetical order
                termsAggregation.KustoQL += $"{KustoQLOperators.CommandSeparator}{KustoQLOperators.OrderBy} ['{termsAggregation.Key}'] {termsAggregation.Order.Sort}";
            }
            else if (termsAggregation.Order.Field == "_count")
            {
                // Count order
                termsAggregation.KustoQL += $"{KustoQLOperators.CommandSeparator}{KustoQLOperators.OrderBy} {BucketColumnNames.Count} {termsAggregation.Order.Sort}";
            }
            else
            {
                // Custom order
                termsAggregation.KustoQL += $"{KustoQLOperators.CommandSeparator}{KustoQLOperators.OrderBy} ['{termsAggregation.Order.Field}'] {termsAggregation.Order.Sort}";
            }

            termsAggregation.KustoQL += $"{KustoQLOperators.CommandSeparator}{KustoQLOperators.Limit} {termsAggregation.Size}";
        }
    }
}
