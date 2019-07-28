namespace K2Bridge.KustoConnector
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Linq;
    using System.Reflection;
    using Kusto.Cloud.Platform.Data;
    using Kusto.Data;
    using Kusto.Data.Common;
    using Kusto.Data.Net.Client;
    using Microsoft.Extensions.Configuration;
    using Newtonsoft.Json;

    public class KustoManager
    {
        private readonly ICslQueryProvider client;

        public KustoManager(IConfigurationRoot config)
        {
            KustoConnectionStringBuilder conn = new KustoConnectionStringBuilder(config["kustoClusterUrl"], config["kustoDatabase"])
                .WithAadApplicationKeyAuthentication(config["kustoAadClientId"], config["kustoAadClientSecret"], config["kustoAadTenantId"]);

            this.client = KustoClientFactory.CreateCslQueryProvider(conn);
        }

        public ElasticResponse ExecuteQuery(string query)
        {
            IDataReader reader;

            //reader = this.client.ExecuteQuery(query);

            //var dataTables = new List<DataTable>();

            //while (!reader.IsClosed)
            //{
            //    var dataTable = new DataTable();
            //    dataTable.Load(reader);
            //    dataTables.Add(dataTable);
            //}

            //var dataTableMappings = this.GetDataTableMapping(dataTables[dataTables.Count - 1]);

            //var hitsTable = dataTables[dataTableMappings.Where(x => x.PrettyName == "hits").Single().Ordinal];
            //var aggsTable = dataTables[dataTableMappings.Where(x => x.PrettyName == "aggs").Single().Ordinal];

            //foreach (DataRow item in hitsTable.Rows)
            //{
            //    var x = CreateItemFromRow<Hit>(item);
            //}

            reader = this.client.ExecuteQuery(query);
            ElasticResponse response = JsonConvert.DeserializeObject<ElasticResponse>("{\"responses\":[{\"took\":57,\"timed_out\":false,\"_shards\":{\"total\":1,\"successful\":1,\"skipped\":0,\"failed\":0},\"hits\":{\"total\":10,\"max_score\":null,\"hits\":[{\"_index\":\"kibana_sample_data_flights\",\"_type\":\"_doc\",\"_id\":\"YKWU-2sBj-CT-0a9s8Hq\",\"_version\":1,\"_score\":null,\"_source\":{\"FlightNum\":\"OH2HNJQ\",\"DestCountry\":\"GB\",\"OriginWeather\":\"Sunny\",\"OriginCityName\":\"San Juan\",\"AvgTicketPrice\":645.1702827323709,\"DistanceMiles\":4128.506536666042,\"FlightDelay\":false,\"DestWeather\":\"Damaging Wind\",\"Dest\":\"Manchester Airport\",\"FlightDelayType\":\"No Delay\",\"OriginCountry\":\"PR\",\"dayOfWeek\":1,\"DistanceKilometers\":6644.187223744275,\"timestamp\":\"2019-07-23T14:04:43\",\"DestLocation\":{\"lat\":\"53.35369873\",\"lon\":\"-2.274950027\"},\"DestAirportID\":\"MAN\",\"Carrier\":\"ES-Air\",\"Cancelled\":false,\"FlightTimeMin\":442.945814916285,\"Origin\":\"Luis Munoz Marin International Airport\",\"OriginLocation\":{\"lat\":\"18.43939972\",\"lon\":\"-66.00180054\"},\"DestRegion\":\"GB-ENG\",\"OriginAirportID\":\"SJU\",\"OriginRegion\":\"PR-U-A\",\"DestCityName\":\"Manchester\",\"FlightTimeHour\":7.3824302486047495,\"FlightDelayMin\":0},\"fields\":{\"hour_of_day\":[14],\"timestamp\":[\"2019-07-23T14:04:43.000Z\"]},\"sort\":[1563890683000]}]},\"aggregations\":{\"2\":{\"buckets\":[{\"key_as_string\":\"2019-07-08T00:00:00.000+02:00\",\"key\":1562536800000,\"doc_count\":318},{\"key_as_string\":\"2019-07-09T00:00:00.000+02:00\",\"key\":1562623200000,\"doc_count\":295}]}},\"status\":200}]}");

            int tableOrdinal = 0;

            do
            {
                switch (tableOrdinal)
                {
                    case 1:
                        KustoParser.ReadHits(reader, response);
                        break;
                    case 0:
                        KustoParser.ReadAggs(reader, response);
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

        // function that creates an object from the given data row
        public static T CreateItemFromRow<T>(DataRow row)
            where T : new()
        {
            // create a new object
            T item = new T();

            // set the item
            SetItemFromRow(item, row);

            // return 
            return item;
        }

        public static void SetItemFromRow<T>(T item, DataRow row)
            where T : new()
        {
            // go through each column
            foreach (DataColumn c in row.Table.Columns)
            {
                // find the property for the column
                PropertyInfo p = item.GetType().GetProperty(c.ColumnName);

                // if exists, set the value
                if (p != null && row[c] != DBNull.Value)
                {
                    p.SetValue(item, row[c], null);
                }
            }
        }

        private List<DataTableMapping> GetDataTableMapping(DataTable dt)
        {
            var convertedList = (from rw in dt.AsEnumerable()
                                 select new DataTableMapping()
                                 {
                                     Ordinal = Convert.ToInt32(rw["Ordinal"]),
                                     Kind = (string)rw["Kind"],
                                     Name = (string)rw["Name"],
                                     Id = (string)rw["Id"],
                                     PrettyName = (string)rw["PrettyName"],
                                 }).ToList();

            return convertedList;
        }

        private class DataTableMapping
        {
            public int Ordinal { get; set; }

            public string Kind { get; set; }

            public string Name { get; set; }

            public string Id { get; set; }

            public string PrettyName { get; set; }
        }


    }
}
