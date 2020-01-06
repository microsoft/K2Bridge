// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2Bridge.KustoConnector
{
    using System;
    using System.Data;
    using K2Bridge.Models;
    using Kusto.Data;
    using Kusto.Data.Common;
    using Kusto.Data.Net.Client;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// The kusto manager handles the connection to the kusto cluster.
    /// </summary>
    internal class KustoManager : IQueryExecutor
    {
        private const string KustoApplicationNameForTracing = "K2Bridge";
        private static readonly string ASSEMBLYVERSION = typeof(KustoManager).Assembly.GetName().Version.ToString();
        private readonly ICslQueryProvider queryClient;
        private readonly ICslAdminProvider adminClient;
        private readonly ILogger<KustoManager> logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="KustoManager"/> class.
        /// </summary>
        /// <param name="connectionDetails">Kusto Connection Details.</param>
        /// <param name="loggerFactory">A logger.</param>
        public KustoManager(KustoConnectionDetails connectionDetails, ILoggerFactory loggerFactory)
        {
            logger = loggerFactory.CreateLogger<KustoManager>();

            var conn = new KustoConnectionStringBuilder(
                connectionDetails.KustoClusterUrl,
                connectionDetails.KustoDatabase)
                .WithAadApplicationKeyAuthentication(
                    connectionDetails.KustoAadClientId,
                    connectionDetails.KustoAadClientSecret,
                    connectionDetails.KustoAadTenantId);

            conn.ApplicationNameForTracing = KustoApplicationNameForTracing;
            conn.ClientVersionForTracing = ASSEMBLYVERSION;

            queryClient = KustoClientFactory.CreateCslQueryProvider(conn);
            adminClient = KustoClientFactory.CreateCslAdminProvider(conn);
        }

        /// <summary>
        /// Executes a Control command in Kusto.
        /// </summary>
        /// <param name="command">The command to execute.</param>
        /// <returns>A data reader with a result.</returns>
        public IDataReader ExecuteControlCommand(string command)
        {
            logger.LogDebug("Calling adminClient.ExecuteControlCommand with the command: {@command}", command);
            var result = adminClient.ExecuteControlCommand(command);
            logger.LogDebug("Result: {@result}", result);

            return result;
        }

        /// <summary>
        /// Executes a Monitored Query in Kusto.
        /// </summary>
        /// <param name="queryData">A Query data.</param>
        /// <returns>A data reader with response and time taken.</returns>
        public (TimeSpan timeTaken, IDataReader reader) ExecuteQuery(QueryData queryData)
        {
            logger.LogDebug("Calling queryClient.ExecuteMonitoredQuery with query data: {@queryData}", queryData);

            // Use the kusto client to execute the query
            var (timeTaken, dataReader) = queryClient.ExecuteMonitoredQuery(queryData.KQL);
            logger.LogDebug("DataReader: {@dataReader} columns. timeTaken: {@timeTaken}", dataReader, timeTaken);
            return (timeTaken, dataReader);
        }
    }
}