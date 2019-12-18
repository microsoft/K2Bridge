// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace K2Bridge.KustoConnector
{
    using System;
    using System.Data;
    using System.Linq;
    using K2Bridge.Models;
    using K2Bridge.Models.Response;
    using Kusto.Data;
    using Kusto.Data.Common;
    using Kusto.Data.Net.Client;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// The kusto manager handles the connection to the kusto cluster
    /// </summary>
    internal class KustoManager : IQueryExecutor
    {
        private const string KustoApplicationNameForTracing = "K2Bridge";
        private readonly ICslQueryProvider queryClient;
        private readonly ICslAdminProvider adminClient;
        private readonly ILogger<KustoManager> logger;

        public KustoManager(KustoConnectionDetails connectionDetails, ILoggerFactory loggerFactory)
        {
            this.logger = loggerFactory.CreateLogger<KustoManager>();

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

            this.queryClient = KustoClientFactory.CreateCslQueryProvider(conn);
            this.adminClient = KustoClientFactory.CreateCslAdminProvider(conn);
        }

        public IDataReader ExecuteControlCommand(string command)
        {
            var result = this.adminClient.ExecuteControlCommand(command);
            this.logger.LogDebug($"Columns received from control command: {result.FieldCount}");

            return result;
        }

        public ElasticResponse ExecuteQuery(QueryData queryData)
        {
            // Use the kusto client to execute the query
            var (timeTaken, reader) = this.queryClient.ExecuteMonitoredQuery(queryData.KQL);
            using (reader)
            {
                try
                {
                    // Read results and transform to Elastic form
                    var response = ReadResponse(queryData, reader, timeTaken);
                    this.logger.LogDebug($"Aggs processed: {response.GetAllAggregations().Count()}");
                    this.logger.LogDebug($"Hits processed: {response.GetAllHits().Count()}");
                    return response;
                }
                catch (Exception ex)
                {
                    this.logger.LogWarning($"Error reading kusto response: {ex.Message}");
                    throw;
                }
            }
        }

        /// <summary>
        /// Use the kusto data reader and build an Elastic response
        /// </summary>
        /// <param name="query"></param>
        /// <param name="reader"></param>
        /// <param name="timeTaken"></param>
        /// <returns></returns>
        private static ElasticResponse ReadResponse(
            QueryData query,
            IDataReader reader,
            TimeSpan timeTaken)
        {
            var response = new ElasticResponse();
            response.AddTook(timeTaken);
            int tableOrdinal = 0;

            do
            {
                switch (tableOrdinal)
                {
                    case 0:
                        foreach (var agg in reader.ReadAggs())
                        {
                            response.AddAggregation(agg);
                        }

                        break;
                    case 1:
                        foreach (var hit in reader.ReadHits(query))
                        {
                            response.AddHit(hit);
                        }

                        break;
                    default:
                        break;
                }

                tableOrdinal++;
            }
            while (reader.NextResult());
            return response;
        }
    }
}