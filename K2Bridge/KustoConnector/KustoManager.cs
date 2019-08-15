namespace K2Bridge.KustoConnector
{
    using System.Data;
    using Kusto.Data;
    using Kusto.Data.Common;
    using Kusto.Data.Net.Client;
    using Microsoft.Extensions.Logging;
    using Newtonsoft.Json;

    internal class KustoManager : IQueryExecutor
    {
        private readonly ICslQueryProvider queryClient;
        private readonly ICslAdminProvider adminClient;
        private readonly ILogger<KustoManager> logger;
        private readonly KustoParser kustoParser;

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

            this.kustoParser = new KustoParser(this.logger);
        }

        public ElasticResponse ExecuteQuery(string query)
        {
            IDataReader reader;
            reader = this.queryClient.ExecuteQuery(query);
            var response = JsonConvert.DeserializeObject<ElasticResponse>("{\"responses\":[{\"took\":57,\"timed_out\":false,\"_shards\":{\"total\":1,\"successful\":1,\"skipped\":0,\"failed\":0},\"hits\":{\"total\":10,\"max_score\":null,\"hits\":[{\"_index\":\"kibana_sample_data_flights\",\"_type\":\"_doc\",\"_id\":\"YKWU-2sBj-CT-0a9s8Hq\",\"_version\":1,\"_score\":null,\"_source\":{\"FlightNum\":\"OH2HNJQ\",\"DestCountry\":\"GB\",\"OriginWeather\":\"Sunny\",\"OriginCityName\":\"San Juan\",\"AvgTicketPrice\":645.1702827323709,\"DistanceMiles\":4128.506536666042,\"FlightDelay\":false,\"DestWeather\":\"Damaging Wind\",\"Dest\":\"Manchester Airport\",\"FlightDelayType\":\"No Delay\",\"OriginCountry\":\"PR\",\"dayOfWeek\":1,\"DistanceKilometers\":6644.187223744275,\"timestamp\":\"2019-07-23T14:04:43\",\"DestLocation\":{\"lat\":\"53.35369873\",\"lon\":\"-2.274950027\"},\"DestAirportID\":\"MAN\",\"Carrier\":\"ES-Air\",\"Cancelled\":false,\"FlightTimeMin\":442.945814916285,\"Origin\":\"Luis Munoz Marin International Airport\",\"OriginLocation\":{\"lat\":\"18.43939972\",\"lon\":\"-66.00180054\"},\"DestRegion\":\"GB-ENG\",\"OriginAirportID\":\"SJU\",\"OriginRegion\":\"PR-U-A\",\"DestCityName\":\"Manchester\",\"FlightTimeHour\":7.3824302486047495,\"FlightDelayMin\":0},\"fields\":{\"hour_of_day\":[14],\"timestamp\":[\"2019-07-23T14:04:43.000Z\"]},\"sort\":[1563890683000]}]},\"aggregations\":{\"2\":{\"buckets\":[{\"key_as_string\":\"2019-07-08T00:00:00.000+02:00\",\"key\":1562536800000,\"doc_count\":318},{\"key_as_string\":\"2019-07-09T00:00:00.000+02:00\",\"key\":1562623200000,\"doc_count\":295}]}},\"status\":200}]}");

            int tableOrdinal = 0;

            do
            {
                switch (tableOrdinal)
                {
                    case 0:
                        this.kustoParser.ReadAggs(reader, response);
                        break;
                    case 1:
                        this.kustoParser.ReadHits(reader, response);
                        break;
                    default:
                        break;
                }

                tableOrdinal++;
            }
            while (reader.NextResult());

            reader.Close();
            return response;
        }

        public IDataReader ExecuteControlCommand(string command)
        {
            var result = this.adminClient.ExecuteControlCommand(command);
            this.logger.LogDebug($"columns received: {result.FieldCount}");

            return result;
        }
    }
}