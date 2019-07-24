using System;
using System.Collections.Generic;
using System.Text;

namespace K2Bridge.KustoConnector
{
    using Kusto.Data;
    using Kusto.Data.Common;
    using Kusto.Data.Net.Client;
    using Kusto.Data.Results;
    using Microsoft.Extensions.Configuration;
    using Newtonsoft.Json;

    public class KustoManager
    {
        private ICslQueryProvider client;

        public KustoManager()
        {
            // TODO: Move the configuration initialization to a centralized place
            var config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", false, true)
                .Build();

            KustoConnectionStringBuilder conn = new KustoConnectionStringBuilder("https://kustolab.kusto.windows.net", "takamara")
                .WithAadApplicationKeyAuthentication(config["kustoAadClientId"], config["kustoAadClientSecret"], config["kustoAadTenantId"]);

            this.client = KustoClientFactory.CreateCslQueryProvider(conn);
        }

        public ElasticResponse ExecuteQuery(string query)
        {
            //var queryResult = this.client.ExecuteQuery(
            //   "(kibana_sample_data_flights | summarize count() | as aggs);(kibana_sample_data_flights | take 10 | as hits)");
            var queryResult = this.client.ExecuteQuery(query);
            ElasticResponse response = JsonConvert.DeserializeObject<ElasticResponse>("{\"responses\":[{\"took\":57,\"timed_out\":false,\"_shards\":{\"total\":1,\"successful\":1,\"skipped\":0,\"failed\":0},\"hits\":{\"total\":10,\"max_score\":null,\"hits\":[{\"_index\":\"kibana_sample_data_flights\",\"_type\":\"_doc\",\"_id\":\"YKWU-2sBj-CT-0a9s8Hq\",\"_version\":1,\"_score\":null,\"_source\":{\"FlightNum\":\"OH2HNJQ\",\"DestCountry\":\"GB\",\"OriginWeather\":\"Sunny\",\"OriginCityName\":\"San Juan\",\"AvgTicketPrice\":645.1702827323709,\"DistanceMiles\":4128.506536666042,\"FlightDelay\":false,\"DestWeather\":\"Damaging Wind\",\"Dest\":\"Manchester Airport\",\"FlightDelayType\":\"No Delay\",\"OriginCountry\":\"PR\",\"dayOfWeek\":1,\"DistanceKilometers\":6644.187223744275,\"timestamp\":\"2019-07-23T14:04:43\",\"DestLocation\":{\"lat\":\"53.35369873\",\"lon\":\"-2.274950027\"},\"DestAirportID\":\"MAN\",\"Carrier\":\"ES-Air\",\"Cancelled\":false,\"FlightTimeMin\":442.945814916285,\"Origin\":\"Luis Munoz Marin International Airport\",\"OriginLocation\":{\"lat\":\"18.43939972\",\"lon\":\"-66.00180054\"},\"DestRegion\":\"GB-ENG\",\"OriginAirportID\":\"SJU\",\"OriginRegion\":\"PR-U-A\",\"DestCityName\":\"Manchester\",\"FlightTimeHour\":7.3824302486047495,\"FlightDelayMin\":0},\"fields\":{\"hour_of_day\":[14],\"timestamp\":[\"2019-07-23T14:04:43.000Z\"]},\"sort\":[1563890683000]}]},\"aggregations\":{\"2\":{\"buckets\":[{\"key_as_string\":\"2019-07-08T00:00:00.000+02:00\",\"key\":1562536800000,\"doc_count\":318},{\"key_as_string\":\"2019-07-09T00:00:00.000+02:00\",\"key\":1562623200000,\"doc_count\":295},{\"key_as_string\":\"2019-07-10T00:00:00.000+02:00\",\"key\":1562709600000,\"doc_count\":337},{\"key_as_string\":\"2019-07-11T00:00:00.000+02:00\",\"key\":1562796000000,\"doc_count\":324},{\"key_as_string\":\"2019-07-12T00:00:00.000+02:00\",\"key\":1562882400000,\"doc_count\":346},{\"key_as_string\":\"2019-07-13T00:00:00.000+02:00\",\"key\":1562968800000,\"doc_count\":343},{\"key_as_string\":\"2019-07-14T00:00:00.000+02:00\",\"key\":1563055200000,\"doc_count\":221},{\"key_as_string\":\"2019-07-15T00:00:00.000+02:00\",\"key\":1563141600000,\"doc_count\":335},{\"key_as_string\":\"2019-07-16T00:00:00.000+02:00\",\"key\":1563228000000,\"doc_count\":327},{\"key_as_string\":\"2019-07-17T00:00:00.000+02:00\",\"key\":1563314400000,\"doc_count\":321},{\"key_as_string\":\"2019-07-18T00:00:00.000+02:00\",\"key\":1563400800000,\"doc_count\":307},{\"key_as_string\":\"2019-07-19T00:00:00.000+02:00\",\"key\":1563487200000,\"doc_count\":326},{\"key_as_string\":\"2019-07-20T00:00:00.000+02:00\",\"key\":1563573600000,\"doc_count\":333},{\"key_as_string\":\"2019-07-21T00:00:00.000+02:00\",\"key\":1563660000000,\"doc_count\":231},{\"key_as_string\":\"2019-07-22T00:00:00.000+02:00\",\"key\":1563746400000,\"doc_count\":323},{\"key_as_string\":\"2019-07-23T00:00:00.000+02:00\",\"key\":1563832800000,\"doc_count\":224}]}},\"status\":200}]}");
            while (queryResult.NextResult())
            {
                if (queryResult.GetName(0) == "raw")
                {
                    KustoParser.ReadHits(queryResult, response);
                }
                else
                {
                    KustoParser.ReadAggs(queryResult, response);
                }
            }

            queryResult.Close();

            return response;
        }
    }
}
