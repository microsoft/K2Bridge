// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2Bridge.KustoDAL
{
    using System;
    using System.Data;
    using System.Threading.Tasks;
    using K2Bridge.Factories;
    using K2Bridge.Models;
    using K2Bridge.Models.Response.Metadata;
    using K2Bridge.Utils;
    using K2Bridge.Visitors;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// DAL for Kusto.
    /// </summary>
    internal class KustoDataAccess : IKustoDataAccess
    {
        private const int MaxFieldCapsSampleSize = 8;
        private const string MaxFieldCapsLookbackTime = "4h";
        private const int MaxFieldCapsRecursionDepth = 4;
        private const string MaxFieldCapsUniqueGetSchemaColumn = "K2BridgeGetSchemaColumn";

        /// <summary>
        /// Initializes a new instance of the <see cref="KustoDataAccess"/> class.
        /// </summary>
        /// <param name="kustoClient">Query Executor.</param>
        /// <param name="requestContext">An object that represents properties of the entire request process.</param>
        /// <param name="logger">A logger.</param>
        public KustoDataAccess(IQueryExecutor kustoClient, RequestContext requestContext, ILogger<KustoDataAccess> logger)
        {
            Kusto = kustoClient;
            RequestContext = requestContext;
            Logger = logger;
        }

        private IQueryExecutor Kusto { get; set; }

        private RequestContext RequestContext { get; set; }

        private ILogger Logger { get; set; }

        /// <summary>
        /// Executes a query to Kusto for Fields Caps.
        /// </summary>
        /// <param name="indexName">Index name.</param>
        /// <returns>An object with the field caps.</returns>
        public async Task<FieldCapabilityResponse> GetFieldCapsAsync(string indexName)
        {
            var response = new FieldCapabilityResponse();
            try
            {
                Logger.LogDebug("Getting schema for table '{@indexName}'", indexName);
                var (databaseName, tableName) = KustoDatabaseTableNames.FromElasticIndexName(indexName, Kusto.DefaultDatabaseName);
                string kustoCommand = $".show {KustoQLOperators.Databases} {KustoQLOperators.Schema} | {KustoQLOperators.Where} TableName=='{tableName}' {KustoQLOperators.And} DatabaseName=='{databaseName}' {KustoQLOperators.And} ColumnName!='' | {KustoQLOperators.Project} ColumnName, ColumnType";

                using IDataReader kustoResults = await Kusto.ExecuteControlCommandAsync(kustoCommand, RequestContext);

                await MapFieldCapsAsync(kustoResults, response, tableName);

                if (response.Fields.Count > 0)
                {
                    return response;
                }

                Logger.LogDebug("Getting schema for function '{@indexName}'", indexName);
                string functionQuery = $"{tableName} | {KustoQLOperators.GetSchema} | project ColumnName, ColumnType=DataType";
                var functionQueryData = new QueryData(functionQuery, tableName, null, null);
                var (timeTaken, reader) = await Kusto.ExecuteQueryAsync(functionQueryData, RequestContext);
                await MapFieldCapsAsync(reader, response, tableName);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error while executing GetFieldCaps.");
                throw;
            }

            return response;
        }

        /// <summary>
        /// Executes a query to Kusto for Index List.
        /// Searches for all tables and all functions that match the index name pattern.
        /// </summary>
        /// <param name="indexName">Index name pattern, e.g. "*", "orders*", "orders".</param>
        /// <returns>A list of Indexes matching the given name pattern.</returns>
        public async Task<IndexListResponseElement> GetIndexListAsync(string indexName)
        {
            var response = new IndexListResponseElement();
            try
            {
                Logger.LogDebug("Listing tables matching '{@indexName}'", indexName);
                var (databaseName, tableName) = KustoDatabaseTableNames.FromElasticIndexName(indexName, Kusto.DefaultDatabaseName);
                string readTablesCommand = $".show {KustoQLOperators.Databases} {KustoQLOperators.Schema} | {KustoQLOperators.Where} TableName != '' | {KustoQLOperators.Distinct} TableName, DatabaseName | {KustoQLOperators.Search} TableName: '{tableName}' | {KustoQLOperators.Search} DatabaseName: '{databaseName}' |  {KustoQLOperators.Project} strcat(DatabaseName, \"{KustoDatabaseTableNames.Separator}\", TableName)";

                using IDataReader kustoTables = await Kusto.ExecuteControlCommandAsync(readTablesCommand, RequestContext);
                if (kustoTables != null)
                {
                    MapIndexList(kustoTables, response);
                }

                Logger.LogDebug("Listing functions matching '{@indexName}'", indexName);
                var defaultDb = Kusto.DefaultDatabaseName;
                string readFunctionsCommand = $".show {KustoQLOperators.Functions} | {KustoQLOperators.Where} Parameters == '()' | {KustoQLOperators.Distinct} Name | {KustoQLOperators.Search} Name: '{tableName}' | {KustoQLOperators.Project} strcat(\"{defaultDb}\", \"{KustoDatabaseTableNames.Separator}\", Name)";

                using IDataReader kustoFunctions = await Kusto.ExecuteControlCommandAsync(readFunctionsCommand, RequestContext);
                if (kustoFunctions != null)
                {
                    MapIndexList(kustoFunctions, response);
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error while executing GetIndexList.");
                throw;
            }

            return response;
        }

        private async Task MapFieldCapsAsync(IDataReader kustoResults, FieldCapabilityResponse response, string tableName, int depth = 0)
        {
            if (depth > MaxFieldCapsRecursionDepth)
            {
                return;
            }

            while (kustoResults.Read())
            {
                var fieldCapabilityElement = FieldCapabilityElementFactory.CreateFromDataRecord(kustoResults);

                if (fieldCapabilityElement.Name.EndsWith(MaxFieldCapsUniqueGetSchemaColumn, StringComparison.InvariantCulture))
                {
                    continue;
                }

                if (fieldCapabilityElement.Type == "object")
                {
                    string dynamicQuery = $"{tableName} | {KustoQLOperators.Where} ingestion_time() > ago({MaxFieldCapsLookbackTime}) | {KustoQLOperators.Where} isnotempty({fieldCapabilityElement.Name}) | {KustoQLOperators.Sample} {MaxFieldCapsSampleSize} | {KustoQLOperators.Project} {MaxFieldCapsUniqueGetSchemaColumn}={fieldCapabilityElement.Name} | {KustoQLOperators.Evaluate} bag_unpack({MaxFieldCapsUniqueGetSchemaColumn}) | {KustoQLOperators.GetSchema} | {KustoQLOperators.Project} ColumnName=strcat('{fieldCapabilityElement.Name}','.',ColumnName),ColumnType=DataType";

                    var dynamicQueryData = new QueryData(dynamicQuery, tableName, null, null);
                    IDataReader reader;
                    try
                    {
                        (_, reader) = await Kusto.ExecuteQueryAsync(dynamicQueryData, RequestContext);
                    }
                    catch (K2Bridge.KustoDAL.QueryException)
                    {
                        continue;
                    }

                    await MapFieldCapsAsync(reader, response, tableName, depth + 1);
                }

                if (string.IsNullOrEmpty(fieldCapabilityElement.Type))
                {
                    Logger.LogWarning("Field: {@fieldCapabilityElement} doesn't have a type.", fieldCapabilityElement);
                }

                response.AddField(fieldCapabilityElement);

                Logger.LogDebug("Found field: {@fieldCapabilityElement}", fieldCapabilityElement);
            }
        }

        private void MapIndexList(IDataReader kustoResults, IndexListResponseElement response)
        {
            while (kustoResults.Read())
            {
                var termBucket = TermBucketFactory.CreateFromDataRecord(kustoResults);
                response.Aggregations.IndexCollection.AddBucket(termBucket);

                Logger.LogDebug("Found table/function: {@termBucket}", termBucket);
            }
        }
    }
}
