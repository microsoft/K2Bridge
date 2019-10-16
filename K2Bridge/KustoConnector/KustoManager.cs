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

    internal class KustoManager : IQueryExecutor
    {
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
            using (var reader = this.queryClient.ExecuteQuery(queryData.KQL))
            {
                ElasticResponse response;
                try
                {
                    response = ReadResponse(queryData.IndexName, reader);
                }
                catch (Exception e)
                {
                    this.logger.LogWarning($"Error reading kusto response: {e.Message}");
                    throw;
                }

                this.logger.LogDebug($"Aggs processed: {response.GetAllAggregations().Count()}");
                this.logger.LogDebug($"Hits processed: {response.GetAllHits().Count()}");
                reader.Close();
                return response;
            }
        }

        private static ElasticResponse ReadResponse(string indexName, IDataReader reader)
        {
            var response = new ElasticResponse();
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
                        foreach (var hit in reader.ReadHits(indexName))
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