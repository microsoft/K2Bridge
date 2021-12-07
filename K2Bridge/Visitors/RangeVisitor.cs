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
            EnsureClause.StringIsNotNullOrEmpty(rangeAggregation.Metric, nameof(RangeAggregation.Metric));
            EnsureClause.StringIsNotNullOrEmpty(rangeAggregation.Field, nameof(RangeAggregation.Field));

            // KustoQL case expression
            // case(
            var caseExpression = $"{KustoQLOperators.Case}(";

            // case() predicates
            // foo > 0 and foo < 800, "0-800"
            foreach (var range in rangeAggregation.Ranges)
            {
                var rangeClause = new Models.Request.Queries.RangeClause()
                {
                    FieldName = rangeAggregation.Field,
                    GTEValue = range.From.HasValue ? range.From.ToString() : "*",
                    LTValue = range.To.HasValue ? range.To.ToString() : "*",
                };
                rangeClause.Accept(this);

                var bucketName = $"{range.From}-{range.To}";

                caseExpression += $"{rangeClause.KustoQL}, '{bucketName}', ";
            }

            // End of case() function, with default bucket
            // "default_bucket_name")
            caseExpression += $"'{BucketColumnNames.RangeDefaultBucket}')";

            // KustoQL commands that go after the summarize
            rangeAggregation.KustoQL = $"{rangeAggregation.Metric} by ['{rangeAggregation.Key}'] = {caseExpression}";
        }
    }
}
