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
        private readonly ICslQueryProvider queryClient;
        private readonly ICslAdminProvider adminClient;
        private readonly ILogger<KustoManager> logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="KustoManager"/> class.
        /// </summary>
        /// <param name="connectionDetails"></param>
        /// <param name="loggerFactory"></param>
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
            conn.ClientVersionForTracing =
                typeof(KustoManager).Assembly.GetName().Version.ToString();

            queryClient = KustoClientFactory.CreateCslQueryProvider(conn);
            adminClient = KustoClientFactory.CreateCslAdminProvider(conn);
        }

        public IDataReader ExecuteControlCommand(string command)
        {
            var result = adminClient.ExecuteControlCommand(command);
            logger.LogDebug($"Columns received from control command: {result.FieldCount}");

            return result;
        }

        public (TimeSpan timeTaken, IDataReader reader) ExecuteQuery(QueryData queryData)
        {
            // Use the kusto client to execute the query
            return queryClient.ExecuteMonitoredQuery(queryData.KQL);
        }
    }
}