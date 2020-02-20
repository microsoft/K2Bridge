// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2Bridge.KustoDAL
{
    using System;
    using System.Data;
    using System.Threading.Tasks;
    using K2Bridge.Models;
    using K2Bridge.Models.Response;
    using K2Bridge.Models.Response.Metadata;
    using K2Bridge.Visitors;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// DAL for Kusto.
    /// </summary>
    internal class KustoDataAccess : IKustoDataAccess
    {
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
                MapFieldCaps(kustoResults, response);

                if (response.Fields.Count > 0)
                {
                    return response;
                }

                Logger.LogDebug("Getting schema for function '{@indexName}'", indexName);
                string functionQuery = $"{tableName} | {KustoQLOperators.GetSchema} | project ColumnName, ColumnType=DataType";
                var functionQueryData = new QueryData(functionQuery, tableName, null, null);
                var (timeTaken, reader) = await Kusto.ExecuteQueryAsync(functionQueryData, RequestContext);
                MapFieldCaps(reader, response);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error while executing GetFieldCaps.");
            }

            // TODO: should we return an empty response in case of an error? or just throw?
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
                MapIndexList(kustoTables, response);

                Logger.LogDebug("Listing functions matching '{@indexName}'", indexName);
                var defaultDb = Kusto.DefaultDatabaseName;
                string readFunctionsCommand = $".show {KustoQLOperators.Functions} | {KustoQLOperators.Where} Parameters == '()' | {KustoQLOperators.Distinct} Name | {KustoQLOperators.Search} Name: '{tableName}' | {KustoQLOperators.Project} strcat(\"{defaultDb}\", \"{KustoDatabaseTableNames.Separator}\", Name)";
                using IDataReader kustoFunctions = await Kusto.ExecuteControlCommandAsync(readFunctionsCommand, RequestContext);
                MapIndexList(kustoFunctions, response);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error while executing GetIndexList.");
            }

            // TODO: should we return an empty response in case of an error? or just throw?
            return response;
        }

        private void MapFieldCaps(IDataReader kustoResults, FieldCapabilityResponse response)
        {
            while (kustoResults.Read())
            {
                var fieldCapabilityElement = FieldCapabilityElement.Create(kustoResults);
                if (string.IsNullOrEmpty(fieldCapabilityElement.Type))
                {
                    // TODO add metric
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
                var termBucket = TermBucket.Create(kustoResults);
                response.Aggregations.IndexCollection.AddBucket(termBucket);

                Logger.LogDebug("Found table/function: {@termBucket}", termBucket);
            }
        }
    }
}
