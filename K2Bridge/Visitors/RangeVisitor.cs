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

            // KustoQL case expression
            // case(
            var caseExpression = $"{KustoQLOperators.Case}(";

            // case() predicates
            // foo > 0 and foo < 800, "0-800"
            foreach (var range in rangeAggregation.Ranges)
            {
                var bucketName = string.Empty;

                if (range.From != null)
                {
                    caseExpression += $"{rangeAggregation.FieldName} >= {range.From}";
                    bucketName += range.From.ToString();
                }

                if (range.From != null && range.To != null)
                {
                    caseExpression += " and ";
                }

                bucketName += "-";

                if (range.To != null)
                {
                    caseExpression += $"{rangeAggregation.FieldName} < {range.To}";
                    bucketName += range.To.ToString();
                }

                caseExpression += $", '{bucketName}', ";
            }

            // End of case() function, with default bucket
            // "default_bucket_name")
            caseExpression += $"'{BucketColumnNames.RangeDefaultBucket}')";

            // KustoQL commands that go after the summarize
            rangeAggregation.KustoQL = $"{rangeAggregation.Metric} by ['{rangeAggregation.FieldAlias}'] = {caseExpression}";
        }
    }
}
