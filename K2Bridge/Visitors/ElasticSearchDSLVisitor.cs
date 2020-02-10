// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2Bridge.Visitors
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Threading.Tasks;
    using K2Bridge.DAL;
    using K2Bridge.Models;
    using K2Bridge.Models.Request;

    /// <summary>
    /// Main visitor entry point.
    /// </summary>
    internal partial class ElasticSearchDSLVisitor : IVisitor
    {
        private readonly string defaultDatabaseName;

        private ISchemaRetriever schemaRetriever;

        private ISchemaRetrieverFactory schemaRetrieverFactory;

        public ElasticSearchDSLVisitor(ISchemaRetrieverFactory schemaRetrieverFactory, string defaultDatabaseName = "")
        {
            this.schemaRetrieverFactory = schemaRetrieverFactory;
            this.defaultDatabaseName = defaultDatabaseName;
        }

        /// <summary>
        /// Accept a given visitable object and build the valid Kusto query.
        /// </summary>
        /// <param name="elasticSearchDSL">An Elasticsearch DSL query.</param>
        public void Visit(ElasticSearchDSL elasticSearchDSL)
        {
            Ensure.IsNotNull(elasticSearchDSL, nameof(elasticSearchDSL));

            // Preparing the schema with the index name to be used later
            schemaRetriever = schemaRetrieverFactory.Make(elasticSearchDSL.IndexName);

            // base query
            elasticSearchDSL.Query.Accept(this);

            var queryStringBuilder = new StringBuilder();
            queryStringBuilder.Append($"{KustoQLOperators.Let} fromUnixTimeMilli = (t:long) {{datetime(1970 - 01 - 01) + t * 1millisec}};").Append('\n');

            var (databaseName, tableName) = KustoDatabaseTableNames.FromElasticIndexName(elasticSearchDSL.IndexName, defaultDatabaseName);

            // when an index-pattern doesn't have a default time filter the query element can be empty
            var translatedQueryExpression = !string.IsNullOrEmpty(elasticSearchDSL.Query.KustoQL) ? $"| {elasticSearchDSL.Query.KustoQL}" : string.Empty;
            queryStringBuilder.Append($"{KustoQLOperators.Let} _data = database(\"{databaseName}\").{tableName} {translatedQueryExpression};");

            // aggregations
            // TODO: process the entire list
            if (elasticSearchDSL.Aggregations?.Count > 0)
            {
                queryStringBuilder.Append('\n').Append($"(_data | {KustoQLOperators.Summarize} ");

                foreach (var (_, aggregation) in elasticSearchDSL.Aggregations)
                {
                    aggregation.Accept(this);
                    queryStringBuilder.Append($"{aggregation.KustoQL} ");
                }

                queryStringBuilder.Append("| as aggs);");
            }

            // hits (projections...)
            queryStringBuilder.Append("\n(_data ");
            if (elasticSearchDSL.Size > 0)
            {
                // we only need to sort if we're returning hits
                var orderingList = new List<string>();

                foreach (var sortClause in elasticSearchDSL.Sort)
                {
                    sortClause.Accept(this);
                    if (!string.IsNullOrEmpty(sortClause.KustoQL))
                    {
                        orderingList.Add(sortClause.KustoQL);
                    }
                }

                if (orderingList.Count > 0)
                {
                    queryStringBuilder.Append($"| {KustoQLOperators.OrderBy} {string.Join(", ", orderingList)} ");
                }
            }

            if (elasticSearchDSL.Size >= 0)
            {
                queryStringBuilder.Append($"| {KustoQLOperators.Limit} {elasticSearchDSL.Size} | as hits)");
            }

            elasticSearchDSL.KustoQL = queryStringBuilder.ToString();
        }
    }
}
