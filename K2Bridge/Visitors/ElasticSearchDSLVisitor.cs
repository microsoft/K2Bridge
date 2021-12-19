// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2Bridge.Visitors
{
    using System.Collections.Generic;
    using System.Text;
    using K2Bridge.KustoDAL;
    using K2Bridge.Models.Request;
    using K2Bridge.Models.Request.Aggregations;
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

            var (databaseName, tableName) = KustoDatabaseTableNames.FromElasticIndexName(elasticSearchDSL.IndexName, defaultDatabaseName);

            // when an index-pattern doesn't have a default time filter the query element can be empty
            var translatedQueryExpression = !string.IsNullOrEmpty(elasticSearchDSL.Query.KustoQL) ? $"| {elasticSearchDSL.Query.KustoQL}" : string.Empty;

            // Aggregations
            if (elasticSearchDSL.Query.Bool != null)
            {
                queryStringBuilder.Append($"{KustoQLOperators.Let} _data = database(\"{databaseName}\").{tableName} {translatedQueryExpression};");

                // Aggregations
                if (elasticSearchDSL.Aggregations?.Count > 0)
                {
                    queryStringBuilder.Append('\n').Append("(");

                    foreach (var (_, aggregation) in elasticSearchDSL.Aggregations)
                    {
                        aggregation.Accept(this);
                        queryStringBuilder.Append($"{aggregation.KustoQL} ");
                    }

                    queryStringBuilder.Append("| as aggs);");

                    // We will need the "true" hits count for some aggregations, e.g. Range
                    queryStringBuilder.Append($"\n(_data | {KustoQLOperators.Count} | as hitsTotal);");
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
            }
            else
            {
                // ViewSingleDocument query
                queryStringBuilder.Append($"database(\"{databaseName}\").{tableName} {translatedQueryExpression} | as hits;");
            }

            elasticSearchDSL.KustoQL = queryStringBuilder.ToString();
        }

        private static bool IsFieldDynamic(Aggregation agg)
        {
            return agg.Field.Contains('.');
        }

        // Dynamic fields need their types to be explicitly specified in the query
        private string ConvertDynamicToCorrectType(Aggregation agg)
        {
            if (!IsFieldDynamic(agg))
            {
                return agg.Field;
            }

            return ClauseFieldTypeProcessor.GetType(schemaRetriever, agg.Field).Result switch {
                ClauseFieldType.Date => $"{KustoQLOperators.ToDateTime}({agg.Field})",
                ClauseFieldType.Numeric => $"{KustoQLOperators.ToDouble}({agg.Field})",
                _ => $"{KustoQLOperators.ToStringOperator}({agg.Field})",
            };
        }
    }
}
