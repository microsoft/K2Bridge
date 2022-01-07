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
    /// A visitor for the <see cref="FiltersAggregation"/> element.
    /// </content>
    internal partial class ElasticSearchDSLVisitor : IVisitor
    {
        /// <inheritdoc/>
        public void Visit(FiltersAggregation filtersAggregation)
        {
            Ensure.IsNotNull(filtersAggregation, nameof(FiltersAggregation));

            var queryStringBuilder = new StringBuilder();

            // Part 1:
            // _data | extend ['key'] = pack_array("k1", "k2", "k3"), ['_filter_value']=pack_array(expr1, expr2, expr3)

            // Start of query, until first pack_array()
            queryStringBuilder.Append($"{KustoTableNames.Data} | {KustoQLOperators.Extend} {EncodeKustoField(filtersAggregation.Key)} = {KustoQLOperators.PackArray}(");

            // Insert filters names
            foreach (var filter in filtersAggregation.Filters)
            {
                queryStringBuilder.Append($"'{filter.Key}',");
            }

            // Remove final comma
            queryStringBuilder.Remove(queryStringBuilder.Length - 1, 1);

            // Close the first pack_array() and start the second pack_array()
            queryStringBuilder.Append($"), ['_filter_value'] = {KustoQLOperators.PackArray}(");

            // Insert filters expressions
            foreach (var filter in filtersAggregation.Filters)
            {
                filter.Value.BoolQuery.Accept(this);
                queryStringBuilder.Append($"{filter.Value.BoolQuery.KustoQL},");
            }

            // Remove final comma
            queryStringBuilder.Remove(queryStringBuilder.Length - 1, 1);

            // End part 1
            queryStringBuilder.Append(")");

            // Part 2 is expansion and filtering of rows
            queryStringBuilder.Append($" | {KustoQLOperators.MvExpand} {EncodeKustoField(filtersAggregation.Key)} to typeof(string), ['_filter_value']");
            queryStringBuilder.Append($" | {KustoQLOperators.Where} ['_filter_value'] == true");

            // Part 3 is the summarize part for metrics
            queryStringBuilder.Append($" | {KustoQLOperators.Summarize} {filtersAggregation.SubAggregationsKustoQL}{filtersAggregation.Metric} by {EncodeKustoField(filtersAggregation.Key)}");

            // Order rows by key
            queryStringBuilder.Append($" | {KustoQLOperators.OrderBy} {EncodeKustoField(filtersAggregation.Key)} asc");

            filtersAggregation.KustoQL = queryStringBuilder.ToString();
        }
    }
}
