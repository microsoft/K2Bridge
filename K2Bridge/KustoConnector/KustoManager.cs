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

        /// <summary>
        /// Initializes a new instance of the <see cref="KustoManager"/> class.
        /// </summary>
        /// <param name="connectionDetails">Kusto Connection Details.</param>
        /// <param name="loggerFactory">A logger.</param>
        public KustoManager(IConnectionDetails connectionDetails, ILogger<KustoManager> logger)
        {
            this.Logger = logger;

            var conn = new KustoConnectionStringBuilder(
                connectionDetails.ClusterUrl,
                connectionDetails.DefaultDatabaseName)
                .WithAadApplicationKeyAuthentication(
                    connectionDetails.AadClientId,
                    connectionDetails.AadClientSecret,
                    connectionDetails.AadTenantId);

            conn.ApplicationNameForTracing = KustoApplicationNameForTracing;
            conn.ClientVersionForTracing = ASSEMBLYVERSION;

            queryClient = KustoClientFactory.CreateCslQueryProvider(conn);
            adminClient = KustoClientFactory.CreateCslAdminProvider(conn);
            ConnectionDetails = connectionDetails;
        }

        public IConnectionDetails ConnectionDetails { get; set;}

        private ILogger Logger { get; set; }

        /// <summary>
        /// Executes a Control command in Kusto.
        /// </summary>
        /// <param name="command">The command to execute.</param>
        /// <returns>A data reader with a result.</returns>
        public IDataReader ExecuteControlCommand(string command)
        {
            Logger.LogDebug("Calling adminClient.ExecuteControlCommand with the command: {@command}", command);
            var result = adminClient.ExecuteControlCommand(command);
            return result;
        }

        /// <summary>
        /// Executes a Monitored Query in Kusto.
        /// </summary>
        /// <param name="queryData">A Query data.</param>
        /// <returns>A data reader with response and time taken.</returns>
        public (TimeSpan timeTaken, IDataReader reader) ExecuteQuery(QueryData queryData)
        {
            Logger.LogDebug("Calling queryClient.ExecuteMonitoredQuery with query data: {@queryData}", queryData);

            // Use the kusto client to execute the query
            var (timeTaken, dataReader) = queryClient.ExecuteMonitoredQuery(queryData.KQL);
            var fieldCount = dataReader.FieldCount;
            Logger.LogDebug("FieldCount: {@fieldCount}. timeTaken: {@timeTaken}", fieldCount, timeTaken);
            return (timeTaken, dataReader);
        }
    }
}