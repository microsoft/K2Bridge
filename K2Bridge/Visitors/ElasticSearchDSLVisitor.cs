// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2Bridge.Visitors
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using K2Bridge.KustoDAL;
    using K2Bridge.Models.Request;
    using K2Bridge.Models.Request.Aggregations.Metric;
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

            // Base query
            elasticSearchDSL.Query.Accept(this);

            var queryStringBuilder = new StringBuilder();

            var (databaseName, tableName) = KustoDatabaseTableNames.FromElasticIndexName(elasticSearchDSL.IndexName, defaultDatabaseName);

            // When an index-pattern doesn't have a default time filter the query element can be empty
            var translatedQueryExpression = !string.IsNullOrEmpty(elasticSearchDSL.Query.KustoQL) ? $"| {elasticSearchDSL.Query.KustoQL}" : string.Empty;

            if (elasticSearchDSL.Query.Bool != null)
            {
                queryStringBuilder.Append($"{KustoQLOperators.Let} {KustoTableNames.Data} = database(\"{databaseName}\").{tableName.QuoteKustoTable()} {translatedQueryExpression};");

                // Aggregations
                if (elasticSearchDSL.Aggregations?.Count > 0)
                {
                    elasticSearchDSL.Aggregations.Accept(this);
                    queryStringBuilder.Append(elasticSearchDSL.Aggregations.KustoQL);
                }

                // We will need the "true" hits count for some aggregations, e.g. Range
                // And this line must be added even there is no aggregation (default count metric)
                // KQL ==> (_data | count | as hitsTotal);
                queryStringBuilder.Append($"\n({KustoTableNames.Data} | {KustoQLOperators.Count} | as {KustoTableNames.HitsTotal});");

                // Hits (projections...)
                // The size is deserialized property
                // therefore we check 'Size >= 0' to protect the query.
                if (elasticSearchDSL.Size >= 0)
                {
                    queryStringBuilder.Append($"\n({KustoTableNames.Data} ");

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

                    queryStringBuilder.Append($"| {KustoQLOperators.Limit} {elasticSearchDSL.Size} | as {KustoTableNames.Hits})");
                }
            }
            else
            {
                // ViewSingleDocument query
                queryStringBuilder.Append($"database(\"{databaseName}\").{tableName.QuoteKustoTable()} {translatedQueryExpression} | as {KustoTableNames.Hits};");
            }

            elasticSearchDSL.KustoQL = queryStringBuilder.ToString();
        }

        private static string QuoteKustoField(string field) => field == null ? null : string.Join(".", field.Split(".").Select(s => $"['{s}']"));

        private static bool IsFieldDynamic(string field) => field?.Contains('.') ?? false;

        // Aggregations always need to be wrapped in a type
        private string EncodeKustoField(MetricAggregation agg) => EncodeKustoField(agg.Field, true);

        private string EncodeKustoField(string field, bool wrapDynamic = false)
        {
            var quoted = QuoteKustoField(field);
            if (!IsFieldDynamic(field) || !wrapDynamic)
            {
                return quoted;
            }

            // Dynamic fields need their types to be explicitly specified in the query if in aggregation
            return ClauseFieldTypeProcessor.GetType(schemaRetriever, field).Result switch
            {
                ClauseFieldType.Date => $"{KustoQLOperators.ToDateTime}({quoted})",
                ClauseFieldType.Numeric => $"{KustoQLOperators.ToDouble}({quoted})",
                _ => $"{KustoQLOperators.ToStringOperator}({quoted})",
            };
        }
    }
}
