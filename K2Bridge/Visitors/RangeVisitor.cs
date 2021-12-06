// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2Bridge.Visitors
{
    using System.Globalization;
    using K2Bridge.Models.Request.Aggregations;
    using K2Bridge.Models.Response;

    /// <content>
    /// A visitor for the <see cref="RangeAggregation"/> element.
    /// </content>
    internal partial class ElasticSearchDSLVisitor : IVisitor
    {
        /// <inheritdoc/>
        public void Visit(RangeAggregation rangeAggregation)
        {
            Ensure.IsNotNull(rangeAggregation, nameof(TermsAggregation));
            EnsureClause.StringIsNotNullOrEmpty(rangeAggregation.Metric, nameof(TermsAggregation.Metric));
            EnsureClause.StringIsNotNullOrEmpty(rangeAggregation.FieldName, nameof(TermsAggregation.FieldName));

            // KustoQL commands that go before the summarize
            // | extend bucket_name=case(
            rangeAggregation.KustoQLPreSummarize = $"{KustoQLOperators.CommandSeparator}{KustoQLOperators.Extend} {BucketColumnNames.RangeBucketName}={KustoQLOperators.Case}(";

            // case() predicates
            // foo > 0 and foo < 800, "0-800"
            foreach (var range in rangeAggregation.Ranges)
            {
                var bucketName = string.Empty;

                if (range.From != null)
                {
                    rangeAggregation.KustoQLPreSummarize += $"{rangeAggregation.FieldName} >= {range.From}";
                    bucketName += range.From.ToString();
                }

                if (range.From != null && range.To != null)
                {
                    rangeAggregation.KustoQLPreSummarize += " and ";
                }

                bucketName += "-";

                if (range.To != null)
                {
                    rangeAggregation.KustoQLPreSummarize += $"{rangeAggregation.FieldName} < {range.To}";
                    bucketName += range.To.ToString();
                }

                rangeAggregation.KustoQLPreSummarize += $", '{bucketName}', ";
            }

            // End of case() function, with default bucket
            // "default_bucket_name")
            rangeAggregation.KustoQLPreSummarize += $"'{BucketColumnNames.RangeDefaultBucket}')";

            // KustoQL commands that go after the summarize
            rangeAggregation.KustoQL = $"{rangeAggregation.Metric} by ['{rangeAggregation.FieldAlias}'] = {BucketColumnNames.RangeBucketName}";
        }
    }
}
