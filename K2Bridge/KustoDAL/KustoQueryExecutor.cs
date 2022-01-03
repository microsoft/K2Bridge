// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2Bridge.KustoDAL
{
    using System;
    using System.Data;
    using System.Threading.Tasks;
    using K2Bridge.Models;
    using K2Bridge.Telemetry;
    using Kusto.Data;
    using Kusto.Data.Common;
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
        /// <param name="adminClient">Admin client.</param>
        /// <param name="queryClient">Query client.</param>
        /// <param name="logger">A logger.</param>
        /// <param name="metricsHistograms">The instance of the class to record metrics.</param>
        public KustoQueryExecutor(
            ICslQueryProvider queryClient,
            ICslAdminProvider adminClient,
            ILogger<KustoQueryExecutor> logger,
            Metrics metricsHistograms)
        {
            Logger = logger;
            this.queryClient = queryClient ?? throw new ArgumentNullException(nameof(queryClient));
            this.adminClient = adminClient ?? throw new ArgumentNullException(nameof(adminClient));
            this.metricsHistograms = metricsHistograms;
        }

        /// <inheritdoc/>
        public string DefaultDatabaseName => queryClient.DefaultDatabaseName;

        private ILogger Logger { get; set; }

        /// <summary>
        /// A helper method to create <see cref="KustoConnectionStringBuilder"/>.
        /// </summary>
        /// <param name="connectionDetails">Connection deatails.</param>
        /// <returns>A new instance of <see cref="KustoConnectionStringBuilder"/>.</returns>
        public static KustoConnectionStringBuilder CreateKustoConnectionStringBuilder(IConnectionDetails connectionDetails)
        {
            var conn = new KustoConnectionStringBuilder(
                connectionDetails.ClusterUrl,
                connectionDetails.DefaultDatabaseName)
                .WithAadApplicationKeyAuthentication(
                    connectionDetails.AadClientId,
                    connectionDetails.AadClientSecret,
                    connectionDetails.AadTenantId);

            // Sending both name and version this way for better visibility in Kusto audit logs.
            conn.ApplicationNameForTracing = $"{KustoApplicationNameForTracing}:{AssemblyVersion}";

            return conn;
        }

        /// <summary>
        /// Executes a Control command in Kusto.
        /// </summary>
        /// <param name="command">The command to execute.</param>
        /// <param name="requestContext">An object that represents properties of the entire request process.</param>
        /// <returns>A data reader with a result.</returns>
        public async Task<IDataReader> ExecuteControlCommandAsync(string command, RequestContext requestContext)
        {
            // TODO: When a single K2 flow will generate multiple requests to Kusto - find a way to differentiate them using different ClientRequestIds
            var clientRequestProperties = ClientRequestPropertiesExtensions.ConstructClientRequestPropertiesFromRequestContext(KustoApplicationNameForTracing, ControlCommandActivityName, requestContext);

            Logger.LogDebug("Calling adminClient.ExecuteControlCommand with the command: {@command}", command.ToSensitiveData());
            var result = await adminClient.ExecuteControlCommandAsync(string.Empty, command, clientRequestProperties);
            return result;
        }

        /// <summary>
        /// Executes a Monitored Query in Kusto.
        /// </summary>
        /// <param name="queryData">A Query data.</param>
        /// <param name="requestContext">An object that represents properties of the entire request process.</param>
        /// <returns>A data reader with response and time taken.</returns>
        /// <exception cref="QueryException">Throws a QueryException on error.</exception>
        public async Task<(TimeSpan timeTaken, IDataReader reader)> ExecuteQueryAsync(QueryData queryData, RequestContext requestContext)
        {
            try
            {
                // TODO: When a single K2 flow will generate multiple requests to Kusto - find a way to differentiate them using different ClientRequestIds
                var clientRequestProperties = ClientRequestPropertiesExtensions.ConstructClientRequestPropertiesFromRequestContext(KustoApplicationNameForTracing, QueryActivityName, requestContext);

                // Use the kusto client to execute the query
                var (timeTaken, dataReader) = await queryClient.ExecuteMonitoredQueryAsync(queryData.QueryCommandText, clientRequestProperties, metricsHistograms);
                Logger.LogDebug("Calling queryClient.ExecuteMonitoredQuery with query data: {@queryData}", queryData.ToSensitiveData());

                var fieldCount = dataReader.FieldCount;
                Logger.LogDebug("FieldCount: {@fieldCount}", fieldCount);
                Logger.LogDebug("[metric] backend query total (sdk) duration: {timeTaken}", timeTaken);
                return (timeTaken, dataReader);
            }
            catch (Kusto.Data.Exceptions.SemanticException ex)
            {
                Logger.LogError(ex, "Semantic exception - Failed to execute query.");

                // If it was a semantic error AND it is about an invalid field, ignore
                // the error and return an empty results set, as this is the behaviour of
                // Kibana
                if (!ex.Message.Contains(
                    "failed to resolve scalar expression",
                    StringComparison.OrdinalIgnoreCase))
                {
                    throw new QueryException("Failed executing Azure Data Explorer (ADX/Kusto) query", ex);
                }

                Logger.LogError(ex, "Semantic exception - Failed to execute query.");
                Logger.LogWarning("Returning empty results set.");

                // Empty results set
                var emptyTable = new DataTable();
                return (TimeSpan.FromSeconds(0), emptyTable.CreateDataReader());
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Failed to execute query.");
                throw new QueryException("Failed executing Azure Data Explorer (ADX/Kusto) query", ex);
            }
        }
    }
}
