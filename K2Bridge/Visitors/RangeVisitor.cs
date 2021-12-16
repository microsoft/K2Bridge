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
            Ensure.IsNotNull(rangeAggregation, nameof(RangeAggregation));
            EnsureClause.StringIsNotNullOrEmpty(rangeAggregation.Metric, nameof(RangeAggregation.Metric));
            EnsureClause.StringIsNotNullOrEmpty(rangeAggregation.Field, nameof(RangeAggregation.Field));

            // Start the union operator
            rangeAggregation.KustoQL += "union ";

            // Query expressions for each range
            // (_data | where foo >= 1 and bar < 10 | summarize ['3']=avg(baz), count() | extend ['2'] = '1-10')
            //          ^^^^^^^^^^^^^^^^^^^^^^^^^^^
            //          The range clause
            //                                        ^^^^^^^^^^^^^^^^^^^^^^^^
            //                                        The metrics expressions
            //                                                                  ^^^^^^^
            //                                                                  Default metric count()
            //                                                                            ^^^^^^^^^^^^^^^^^^^^^
            //                                                                            Column with the range name
            foreach (var range in rangeAggregation.Ranges)
            {
                rangeAggregation.KustoQL += $"(_data | {KustoQLOperators.Where} ";

                range.Field = ConvertDynamicToCorrectType(rangeAggregation);
                range.Accept(this);

                // Insert the range clause
                if (string.IsNullOrEmpty(range.KustoQL))
                {
                    // This is then "open" range, -infinity to +infinity
                    rangeAggregation.KustoQL += "true";
                }
                else
                {
                    rangeAggregation.KustoQL += range.KustoQL;
                }

                // Insert the KustoQL generated by the metrics aggregations
                // rangeAggregation.MetricsKustoQL contains the KustoQL generated by the metrics sub-aggregations (if any)
                // rangeAggregation.Metric is the default aggregation, always present, currently count()
                rangeAggregation.KustoQL += $" | {KustoQLOperators.Summarize} {rangeAggregation.SubAggregationsKustoQL}{rangeAggregation.Metric}";

                // Create the column with the range name using extend
                var bucketName = $"{range.From}-{range.To}";
                rangeAggregation.KustoQL += $" | {KustoQLOperators.Extend} ['{rangeAggregation.Key}'] = '{bucketName}'), ";
            }

            // Remove final comma and space
            rangeAggregation.KustoQL = rangeAggregation.KustoQL.TrimEnd(' ', ',');

            // Re-order columns by ascending order
            // Make sure the aggregation column (with range names) is first
            rangeAggregation.KustoQL += $" | {KustoQLOperators.ProjectReorder} ['{rangeAggregation.Key}'], * asc";
        }
    }
}
