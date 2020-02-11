// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2Bridge.KustoConnector
{
    using System;
    using System.Data;
    using System.Threading.Tasks;
    using K2Bridge.Models;
    using K2Bridge.Telemetry;
    using Kusto.Data;
    using Kusto.Data.Common;
    using Kusto.Data.Net.Client;

    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Handles the connection to the kusto cluster and executes queries.
    /// </summary>
    internal class KustoQueryExecutor : IQueryExecutor
    {
        private const string KustoApplicationNameForTracing = "K2Bridge";
        private const string ControlCommandActivityName = "ExecuteControlCommand";
        private const string QueryActivityName = "ExecuteQuery";
        private static readonly string AssemblyVersion = typeof(KustoQueryExecutor).Assembly.GetName().Version.ToString();
        private readonly ICslQueryProvider queryClient;
        private readonly ICslAdminProvider adminClient;
        private readonly Metrics metricsHistograms;

        /// <summary>
        /// Initializes a new instance of the <see cref="KustoQueryExecutor"/> class.
        /// </summary>
        /// <param name="connectionDetails">Kusto Connection Details.</param>
        /// <param name="logger">A logger.</param>
        /// <param name="metricsHistograms">The instance of the class to record metrics.</param>
        public KustoQueryExecutor(
            IConnectionDetails connectionDetails,
            ILogger<KustoQueryExecutor> logger,
            Metrics metricsHistograms)
        {
            Logger = logger;

            var conn = new KustoConnectionStringBuilder(
                connectionDetails.ClusterUrl,
                connectionDetails.DefaultDatabaseName)
                .WithAadApplicationKeyAuthentication(
                    connectionDetails.AadClientId,
                    connectionDetails.AadClientSecret,
                    connectionDetails.AadTenantId);

            // Sending both name and version this way for better visibility in Kusto audit logs.
            conn.ApplicationNameForTracing = $"{KustoApplicationNameForTracing}:{AssemblyVersion}";

            logger.LogTrace("Creating new kusto clients");
            queryClient = KustoClientFactory.CreateCslQueryProvider(conn);
            adminClient = KustoClientFactory.CreateCslAdminProvider(conn);
            ConnectionDetails = connectionDetails;
            this.metricsHistograms = metricsHistograms;
        }

        /// <inheritdoc/>
        public IConnectionDetails ConnectionDetails { get; set; }

        private ILogger Logger { get; set; }

        /// <summary>
        /// Executes a Control command in Kusto.
        /// </summary>
        /// <param name="command">The command to execute.</param>
        /// <param name="requestContext">An object that represents properties that will be sent to Kusto.</param>
        /// <returns>A data reader with a result.</returns>
        public async Task<IDataReader> ExecuteControlCommandAsync(string command, RequestContext requestContext)
        {
            // TODO: When a single K2 flow will generate multiple requests to Kusto - find a way to differentiate them using different ClientRequestIds
            var clientRequestProperties = ClientRequestPropertiesExtensions.ConstructClientRequestPropertiesFromRequestContext(KustoApplicationNameForTracing, ControlCommandActivityName, requestContext);

            Logger.LogDebug("Calling adminClient.ExecuteControlCommand with the command: {@command}", command);
            var result = await adminClient.ExecuteControlCommandAsync(string.Empty, command, clientRequestProperties);
            return result;
        }

        /// <summary>
        /// Executes a Monitored Query in Kusto.
        /// </summary>
        /// <param name="queryData">A Query data.</param>
        /// <param name="requestContext">An object that represents properties that will be sent to Kusto.</param>
        /// <returns>A data reader with response and time taken.</returns>
        public async Task<(TimeSpan timeTaken, IDataReader reader)> ExecuteQueryAsync(QueryData queryData, RequestContext requestContext)
        {
            // TODO: When a single K2 flow will generate multiple requests to Kusto - find a way to differentiate them using different ClientRequestIds
            var clientRequestProperties = ClientRequestPropertiesExtensions.ConstructClientRequestPropertiesFromRequestContext(KustoApplicationNameForTracing, QueryActivityName, requestContext);

            // Use the kusto client to execute the query
            var (timeTaken, dataReader) = await queryClient.ExecuteMonitoredQueryAsync(queryData.QueryCommandText, clientRequestProperties, metricsHistograms);
            Logger.LogDebug("Calling queryClient.ExecuteMonitoredQuery with query data: {@queryData}", queryData);

            var fieldCount = dataReader.FieldCount;
            Logger.LogDebug("FieldCount: {@fieldCount}", fieldCount);
            Logger.LogDebug("[metric] backend query total (sdk) duration: {timeTaken}", timeTaken);
            return (timeTaken, dataReader);
        }
    }
}