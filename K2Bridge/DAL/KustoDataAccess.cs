// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2Bridge.DAL
{
    using System;
    using System.Data;
    using K2Bridge.KustoConnector;
    using K2Bridge.Models;
    using K2Bridge.Visitors;
    using K2Bridge.Models.Response;
    using K2Bridge.Models.Response.Metadata;
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
        /// <param name="logger">A logger.</param>
        public KustoDataAccess(IQueryExecutor kustoClient, ILogger<KustoDataAccess> logger)
        {
            Logger = logger;
            Kusto = kustoClient;
        }

        private IQueryExecutor Kusto { get; set; }

        private ILogger Logger { get; set; }

        /// <summary>
        /// Executes a query to Kusto for Fields Caps.
        /// </summary>
        /// <param name="indexName">Index name.</param>
        /// <returns>An object with the field caps.</returns>
        public FieldCapabilityResponse GetFieldCaps(string indexName)
        {
            var response = new FieldCapabilityResponse();
            try
            {
                Logger.LogDebug("Index name: {@indexName}", indexName);
                var (databaseName, tableName) = KustoDatabaseTableNames.FromElasticIndexName(indexName, Kusto.ConnectionDetails.DefaultDatabaseName);
                string kustoCommand = $".show {KQLOperators.Databases} {KQLOperators.Schema} | {KQLOperators.Where} TableName=='{tableName}' {KQLOperators.And} DatabaseName=='{databaseName}' {KQLOperators.And} ColumnName!='' | {KQLOperators.Project} ColumnName, ColumnType";
                using (IDataReader kustoResults = Kusto.ExecuteControlCommand(kustoCommand))
                {
                    while (kustoResults.Read())
                    {
                        var record = kustoResults;
                        var fieldCapabilityElement = FieldCapabilityElement.Create(record);
                        if (string.IsNullOrEmpty(fieldCapabilityElement.Type))
                        {
                            Logger.LogWarning("Field: {@fieldCapabilityElement} doesn't have a type.", fieldCapabilityElement);
                        }

                        response.AddField(fieldCapabilityElement);

                        Logger.LogDebug("Found field: {@fieldCapabilityElement}", fieldCapabilityElement);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error while executing GetFieldCaps.");
            }

            return response;
        }

        /// <summary>
        /// Executes a query to Kusto for Index List.
        /// </summary>
        /// <param name="indexName">Index name.</param>
        /// <returns>A list of Indexes.</returns>
        public IndexListResponseElement GetIndexList(string indexName)
        {
            var response = new IndexListResponseElement();
            try
            {
                Logger.LogDebug("Index name: {@indexName}", indexName);
                var (databaseName, tableName) = KustoDatabaseTableNames.FromElasticIndexName(indexName, Kusto.ConnectionDetails.DefaultDatabaseName);
                string kustoCommand = $".show {KQLOperators.Databases} {KQLOperators.Schema} | {KQLOperators.Where} TableName != '' | {KQLOperators.Distinct} TableName, DatabaseName | {KQLOperators.Search} TableName: '{tableName}' | {KQLOperators.Search} DatabaseName: '{databaseName}' |  {KQLOperators.Project} strcat(DatabaseName, \"{KustoDatabaseTableNames.Separator}\", TableName)";
                using (IDataReader kustoResults = Kusto.ExecuteControlCommand(kustoCommand))
                {
                    while (kustoResults.Read())
                    {
                        var record = kustoResults;
                        var termBucket = TermBucket.Create(record);
                        response.Aggregations.IndexCollection.AddBucket(termBucket);

                        Logger.LogDebug("Found index/table: {@termBucket}", termBucket);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error while executing GetIndexList.");
            }

            return response;
        }
    }
}