﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2Bridge.Visitors
{
    using System.Collections.Generic;
    using System.Text;
    using K2Bridge.KustoDAL;
    using K2Bridge.Models.Request;
    using K2Bridge.Utils;

    /// <summary>
    /// Main visitor entry point used to convert an ElasticSearch DSL to Kusto queries.
    /// </summary>
    internal partial class ElasticSearchDSLVisitor : IVisitor
    {
        private readonly string defaultDatabaseName;
        private readonly ISchemaRetrieverFactory schemaRetrieverFactory;

        private ISchemaRetriever schemaRetriever;

        /// <summary>
        /// Initializes a new instance of the <see cref="ElasticSearchDSLVisitor"/> class.
        /// </summary>
        /// <param name="schemaRetrieverFactory">A factory to create a <see cref="ISchemaRetriever"/> used
        /// to fetch table/function schema.</param>
        /// <param name="defaultDatabaseName">The database used to fetch functions.</param>
        public ElasticSearchDSLVisitor(ISchemaRetrieverFactory schemaRetrieverFactory, string defaultDatabaseName = "")
        {
            this.schemaRetrieverFactory = schemaRetrieverFactory;
            this.defaultDatabaseName = defaultDatabaseName;
        }

        /// <inheritdoc/>
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
            // The size is deserialized property
            // therefore we check 'Size >= 0' to protect the query.
            if (elasticSearchDSL.Size >= 0)
            {
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

                queryStringBuilder.Append($"| {KustoQLOperators.Limit} {elasticSearchDSL.Size} | as hits)");
            }

            elasticSearchDSL.KustoQL = queryStringBuilder.ToString();
        }

        /// <inheritdoc/>
        public void Visit(SingleDocumentDsl singleDocumentDsl)
        {
            Ensure.IsNotNull(singleDocumentDsl, nameof(singleDocumentDsl));

            // Preparing the schema with the index name to be used later
            schemaRetriever = schemaRetrieverFactory.Make(singleDocumentDsl.IndexName);

            // base query
            singleDocumentDsl.SingleDocQuery.Accept(this);

            // when an index-pattern doesn't have a default time filter the query element can be empty
            var translatedQueryExpression = !string.IsNullOrEmpty(singleDocumentDsl.SingleDocQuery.KustoQL) ? $"| {singleDocumentDsl.SingleDocQuery.KustoQL}" : string.Empty;
            var (databaseName, tableName) = KustoDatabaseTableNames.FromElasticIndexName(singleDocumentDsl.IndexName, defaultDatabaseName);

            var queryStringBuilder = ConstructBasicQuery();

            queryStringBuilder.Append($"database(\"{databaseName}\").{tableName} {translatedQueryExpression} | as hits;");
            singleDocumentDsl.KustoQL = queryStringBuilder.ToString();
        }

        private static StringBuilder ConstructBasicQuery()
        {
            var queryStringBuilder = new StringBuilder();
            queryStringBuilder.Append($"{KustoQLOperators.Let} fromUnixTimeMilli = (t:long) {{datetime(1970 - 01 - 01) + t * 1millisec}};").Append('\n');

            return queryStringBuilder;
        }
    }
}