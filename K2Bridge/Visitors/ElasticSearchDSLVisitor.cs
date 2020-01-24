// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2Bridge.Visitors
{
    using System.Collections.Generic;
    using System.Text;
    using K2Bridge.Models;
    using K2Bridge.Models.Request;

    /// <summary>
    /// Main visitor entry point.
    /// </summary>
    internal partial class ElasticSearchDSLVisitor : IVisitor
    {
        private readonly string defaultDatabaseName;

        public ElasticSearchDSLVisitor(string defaultDatabaseName = "")
        {
            this.defaultDatabaseName = defaultDatabaseName;
        }

        /// <summary>
        /// Accept a given visitable object and build the valid Kusto query based on that KQL.
        /// </summary>
        /// <param name="elasticSearchDSL">An Elasticsearch DSL query.</param>
        public void Visit(ElasticSearchDSL elasticSearchDSL)
        {
            Ensure.IsNotNull(elasticSearchDSL, nameof(elasticSearchDSL));

            var kqlSb = new StringBuilder();

            kqlSb.Append($"{KQLOperators.Let} fromUnixTimeMilli = (t:long) {{datetime(1970 - 01 - 01) + t * 1millisec}};").Append('\n');

            // base query
            elasticSearchDSL.Query.Accept(this);

            // when an index-pattern doesn't have a default time filter the query element can be empty
            var kqlQueryExpression = !string.IsNullOrEmpty(elasticSearchDSL.Query.KQL) ? $"| {elasticSearchDSL.Query.KQL}" : string.Empty;
            var (databaseName, tableName) = KustoDatabaseTableNames.FromElasticIndexName(elasticSearchDSL.IndexName, defaultDatabaseName);
            kqlSb.Append($"{KQLOperators.Let} _data = database(\"{databaseName}\").{tableName} {kqlQueryExpression};");

            // aggregations
            // TODO: process the entire list
            if (elasticSearchDSL.Aggregations?.Count > 0)
            {
                kqlSb.Append('\n').Append($"(_data | {KQLOperators.Summarize} ");

                foreach (var (_, aggregation) in elasticSearchDSL.Aggregations)
                {
                    aggregation.Accept(this);
                    kqlSb.Append($"{aggregation.KQL} ");
                }

                kqlSb.Append("| as aggs);");
            }

            // hits (projections...)
            kqlSb.Append("\n(_data ");
            if (elasticSearchDSL.Size > 0)
            {
                // we only need to sort if we're returning hits
                var orderingList = new List<string>();

                foreach (var sortClause in elasticSearchDSL.Sort)
                {
                    sortClause.Accept(this);
                    if (!string.IsNullOrEmpty(sortClause.KQL))
                    {
                        orderingList.Add(sortClause.KQL);
                    }
                }

                if (orderingList.Count > 0)
                {
                    kqlSb.Append($"| {KQLOperators.OrderBy} {string.Join(", ", orderingList)} ");
                }
            }

            if (elasticSearchDSL.Size >= 0)
            {
                kqlSb.Append($"| {KQLOperators.Limit} {elasticSearchDSL.Size} | as hits)");
            }

            elasticSearchDSL.KQL = kqlSb.ToString();
        }
    }
}
