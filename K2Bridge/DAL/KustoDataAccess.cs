// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2Bridge.DAL
{
    using System.Data;
    using K2Bridge.KustoConnector;
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
            string kustoCommand = $".show database schema | where TableName=='{indexName}' and ColumnName!='' | project ColumnName, ColumnType";
            using (IDataReader kustoResults = Kusto.ExecuteControlCommand(kustoCommand))
            {
                while (kustoResults.Read())
                {
                    var record = kustoResults;
                    var fieldCapabilityElement = FieldCapabilityElement.Create(record);
                    if (string.IsNullOrEmpty(fieldCapabilityElement.Type))
                    {
                        Logger.LogWarning($"Field: {fieldCapabilityElement.Name} doesn't have a type.");
                    }

                    response.AddField(fieldCapabilityElement);

                    Logger.LogDebug($"Found field: {fieldCapabilityElement.Name} with type: {fieldCapabilityElement.Type}");
                }
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
            using (IDataReader kustoResults = Kusto.ExecuteControlCommand($".show tables | search TableName: '{indexName}' | project TableName"))
            {
                while (kustoResults.Read())
                {
                    var record = kustoResults;
                    var termBucket = TermBucket.Create(record);
                    response.Aggregations.IndexCollection.AddBucket(termBucket);

                    Logger.LogDebug($"Found index/table: {termBucket.Key}");
                }
            }

            return response;
        }
    }
}
