// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2Bridge.Visitors
{
    using System;
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

            var expandColumn = EncodeKustoField("_filter_value");
            var packColumn = EncodeKustoField("_tmp");

            var queryStringBuilder = new StringBuilder();

            // Part 1:
            // _data | extend ['key'] = pack_array("k1", "k2", "k3"), ['_filter_value']=pack_array(expr1, expr2, expr3)

            // Assemble a column name that includes the key, and the different filters, base64-encoded
            var columnName = filtersAggregation.Key;
            foreach (var filter in filtersAggregation.Filters)
            {
                columnName += AggregationsConstants.MetadataSeparator;
                columnName += Convert.ToBase64String(Encoding.Default.GetBytes(filter.Key)).Replace('=', '-');
            }



            // Start of query, until first pack_array()
            queryStringBuilder.Append($"{KustoTableNames.Data} | {KustoQLOperators.Extend} {packColumn} = {KustoQLOperators.PackArray}(");

            // Insert filters names
            foreach (var filter in filtersAggregation.Filters)
            {
                queryStringBuilder.Append($"'{filter.Key}',");
            }

            // Remove final comma
            queryStringBuilder.Remove(queryStringBuilder.Length - 1, 1);

            // Close the first pack_array() and start the second pack_array()
            queryStringBuilder.Append($"), {expandColumn} = {KustoQLOperators.PackArray}(");

            // Insert filters expressions
            foreach (var (_, value) in filtersAggregation.Filters)
            {
                value.BoolQuery.Accept(this);
                queryStringBuilder.Append($"{value.BoolQuery.KustoQL},");
            }

            // Remove final comma
            queryStringBuilder.Remove(queryStringBuilder.Length - 1, 1);

            // End part 1
            queryStringBuilder.Append(')');

            // Part 2 is expansion and filtering of rows
            queryStringBuilder.Append($" | {KustoQLOperators.MvExpand} {packColumn} to typeof(string), {expandColumn}");
            queryStringBuilder.Append($" | {KustoQLOperators.Where} {expandColumn} == true");

            // Part 3 is the summarize part for metrics
            queryStringBuilder.Append($" | {KustoQLOperators.Summarize} {filtersAggregation.SubAggregationsKustoQL}{filtersAggregation.Metric} by {packColumn}");

            // Order rows by key
            queryStringBuilder.Append($" | {KustoQLOperators.OrderBy} {packColumn} asc");

            // Rename the temp column to the final name
            queryStringBuilder.Append($" | {KustoQLOperators.ProjectRename} {EncodeKustoField(columnName)}={packColumn}");

            filtersAggregation.KustoQL = queryStringBuilder.ToString();
        }
    }
}
