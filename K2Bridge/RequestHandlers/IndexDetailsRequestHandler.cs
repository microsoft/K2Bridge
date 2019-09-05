namespace K2Bridge
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using K2Bridge.KustoConnector;
    using Microsoft.Extensions.Logging;
    using Newtonsoft.Json;

    internal class IndexDetailsRequestHandler : KibanaRequestHandler
    {
        public IndexDetailsRequestHandler(HttpListenerContext requestContext, IQueryExecutor kustoClient, string requestId, ILogger logger)
            : base(requestContext, kustoClient, requestId, logger)
        {
        }

        public static bool Mine(string rawUrl, string requestString)
        {
            return rawUrl.StartsWith(@"/.kibana/_mget");
        }

        public string PrepareResponse(string requestInputString)
        {
            try
            {
                Models.Metadata.ElasticDocs requestStream = JsonConvert.DeserializeObject<Models.Metadata.ElasticDocs>(requestInputString);

                Models.Metadata.ElasticDocs elasticOutputStream = JsonConvert.DeserializeObject<Models.Metadata.ElasticDocs>(
                                    "{\"docs\":[{\"index\":\".kibana_1\",\"_type\":\"doc\",\"_id\":\"index-pattern:d3d7af60-4c81-11e8-b3d7-01146121b73d\",\"_version\":3,\"_seq_no\":67,\"_primary_term\":2,\"found\":true,\"_source\":{\"index-pattern\":{\"title\":\"kibana_sample_data_flights\",\"timeFieldName\":\"timestamp\",\"fields\":\"\",\"fieldFormatMap\":\"\"},\"type\":\"index-pattern\",\"migrationVersion\":{\"index-pattern\":\"6.5.0\"},\"updated_at\":\"2019-07-18T13:38:35.278Z\"}}]}");

                string indexPatternID = requestStream.docs[0]._id;

                List<Models.Metadata.Hit> hitsList = PrepareHits(indexPatternID);

                if (hitsList.Count > 0)
                    // Marking as found for Kibana
                    hitsList[0].found = true;

                elasticOutputStream.docs = hitsList.ToArray();

                if (hitsList.Count == 0)
                {
                    this.logger.LogDebug($"Detailed index schemas: Could not locate index. Giving way to Kibana local storage ({requestId}):{indexPatternID}");
                    return null;
                }

                string kustoResultsString = JsonConvert.SerializeObject(elasticOutputStream);

                this.logger.LogDebug($"Detailed index schemas:({requestId})");

                return kustoResultsString;
            }
            catch (Exception)
            {
                this.logger.LogError($"Detailed index schemas:({requestId}):Failed to retrieve indexes");
                throw;
            }
        }
    }
}

/*
URL:
/.kibana/_mget
Request:
{"docs":[{"_id":"index-pattern:d3d7af60-4c81-11e8-b3d7-01146121b73d","_type":"doc"}]}
Elastix Response
{"docs":[{"_index":".kibana_1","_type":"doc","_id":"index-pattern:d3d7af60-4c81-11e8-b3d7-01146121b73d","_version":3,"_seq_no":67,"_primary_term":2,"found":true,"_source":{"index-pattern":{"title":"kibana_sample_data_flights","timeFieldName":"timestamp","fields":"[{\"name\":\"AvgTicketPrice\",\"type\":\"number\",\"count\":1,\"scripted\":false,\"searchable\":true,\"aggregatable\":true,\"readFromDocValues\":true},{\"name\":\"Cancelled\",\"type\":\"boolean\",\"count\":1,\"scripted\":false,\"searchable\":true,\"aggregatable\":true,\"readFromDocValues\":true},{\"name\":\"Carrier\",\"type\":\"string\",\"count\":1,\"scripted\":false,\"searchable\":true,\"aggregatable\":true,\"readFromDocValues\":true},{\"name\":\"Dest\",\"type\":\"string\",\"count\":0,\"scripted\":false,\"searchable\":true,\"aggregatable\":true,\"readFromDocValues\":true},{\"name\":\"DestAirportID\",\"type\":\"string\",\"count\":0,\"scripted\":false,\"searchable\":true,\"aggregatable\":true,\"readFromDocValues\":true},{\"name\":\"DestCityName\",\"type\":\"string\",\"count\":1,\"scripted\":false,\"searchable\":true,\"aggregatable\":true,\"readFromDocValues\":true},{\"name\":\"DestCountry\",\"type\":\"string\",\"count\":1,\"scripted\":false,\"searchable\":true,\"aggregatable\":true,\"readFromDocValues\":true},{\"name\":\"DestLocation\",\"type\":\"geo_point\",\"count\":0,\"scripted\":false,\"searchable\":true,\"aggregatable\":true,\"readFromDocValues\":true},{\"name\":\"DestRegion\",\"type\":\"string\",\"count\":0,\"scripted\":false,\"searchable\":true,\"aggregatable\":true,\"readFromDocValues\":true},{\"name\":\"DestWeather\",\"type\":\"string\",\"count\":0,\"scripted\":false,\"searchable\":true,\"aggregatable\":true,\"readFromDocValues\":true},{\"name\":\"DistanceKilometers\",\"type\":\"number\",\"count\":0,\"scripted\":false,\"searchable\":true,\"aggregatable\":true,\"readFromDocValues\":true},{\"name\":\"DistanceMiles\",\"type\":\"number\",\"count\":0,\"scripted\":false,\"searchable\":true,\"aggregatable\":true,\"readFromDocValues\":true},{\"name\":\"FlightDelay\",\"type\":\"boolean\",\"count\":0,\"scripted\":false,\"searchable\":true,\"aggregatable\":true,\"readFromDocValues\":true},{\"name\":\"FlightDelayMin\",\"type\":\"number\",\"count\":0,\"scripted\":false,\"searchable\":true,\"aggregatable\":true,\"readFromDocValues\":true},{\"name\":\"FlightDelayType\",\"type\":\"string\",\"count\":1,\"scripted\":false,\"searchable\":true,\"aggregatable\":true,\"readFromDocValues\":true},{\"name\":\"FlightNum\",\"type\":\"string\",\"count\":0,\"scripted\":false,\"searchable\":true,\"aggregatable\":true,\"readFromDocValues\":true},{\"name\":\"FlightTimeHour\",\"type\":\"string\",\"count\":0,\"scripted\":false,\"searchable\":true,\"aggregatable\":true,\"readFromDocValues\":true},{\"name\":\"FlightTimeMin\",\"type\":\"number\",\"count\":1,\"scripted\":false,\"searchable\":true,\"aggregatable\":true,\"readFromDocValues\":true},{\"name\":\"Origin\",\"type\":\"string\",\"count\":0,\"scripted\":false,\"searchable\":true,\"aggregatable\":true,\"readFromDocValues\":true},{\"name\":\"OriginAirportID\",\"type\":\"string\",\"count\":0,\"scripted\":false,\"searchable\":true,\"aggregatable\":true,\"readFromDocValues\":true},{\"name\":\"OriginCityName\",\"type\":\"string\",\"count\":3,\"scripted\":false,\"searchable\":true,\"aggregatable\":true,\"readFromDocValues\":true},{\"name\":\"OriginCountry\",\"type\":\"string\",\"count\":1,\"scripted\":false,\"searchable\":true,\"aggregatable\":true,\"readFromDocValues\":true},{\"name\":\"OriginLocation\",\"type\":\"geo_point\",\"count\":0,\"scripted\":false,\"searchable\":true,\"aggregatable\":true,\"readFromDocValues\":true},{\"name\":\"OriginRegion\",\"type\":\"string\",\"count\":0,\"scripted\":false,\"searchable\":true,\"aggregatable\":true,\"readFromDocValues\":true},{\"name\":\"OriginWeather\",\"type\":\"string\",\"count\":0,\"scripted\":false,\"searchable\":true,\"aggregatable\":true,\"readFromDocValues\":true},{\"name\":\"_id\",\"type\":\"string\",\"count\":0,\"scripted\":false,\"searchable\":true,\"aggregatable\":true,\"readFromDocValues\":false},{\"name\":\"_index\",\"type\":\"string\",\"count\":0,\"scripted\":false,\"searchable\":true,\"aggregatable\":true,\"readFromDocValues\":false},{\"name\":\"_score\",\"type\":\"number\",\"count\":0,\"scripted\":false,\"searchable\":false,\"aggregatable\":false,\"readFromDocValues\":false},{\"name\":\"_source\",\"type\":\"_source\",\"count\":0,\"scripted\":false,\"searchable\":false,\"aggregatable\":false,\"readFromDocValues\":false},{\"name\":\"_type\",\"type\":\"string\",\"count\":0,\"scripted\":false,\"searchable\":true,\"aggregatable\":true,\"readFromDocValues\":false},{\"name\":\"dayOfWeek\",\"type\":\"number\",\"count\":0,\"scripted\":false,\"searchable\":true,\"aggregatable\":true,\"readFromDocValues\":true},{\"name\":\"timestamp\",\"type\":\"date\",\"count\":0,\"scripted\":false,\"searchable\":true,\"aggregatable\":true,\"readFromDocValues\":true},{\"name\":\"hour_of_day\",\"type\":\"number\",\"count\":0,\"scripted\":true,\"script\":\"doc['timestamp'].value.hourOfDay\",\"lang\":\"painless\",\"searchable\":true,\"aggregatable\":true,\"readFromDocValues\":false}]","fieldFormatMap":"{\"hour_of_day\":{\"id\":\"number\",\"params\":{\"pattern\":\"00\"}},\"AvgTicketPrice\":{\"id\":\"number\",\"params\":{\"pattern\":\"$0,0.[00]\"}}}"},"type":"index-pattern","migrationVersion":{"index-pattern":"6.5.0"},"updated_at":"2019-07-18T13:38:35.278Z"}}]}
Response
{"docs":[{"_index":".kibana_1","_type":"doc","_id":"index-pattern:d3d7af60-4c81-11e8-b3d7-01146121b73d","_version":0,"_score":0.0,"_source":{"index-pattern":{"title":"kibana_sample_data_flights","timeFieldName":null,"fieldFormatMap":"{\"hour_of_day\":{}}","fields":"[{\"name\":\"raw\",\"type\":\"json\",\"count\":0,\"scripted\":false,\"searchable\":true,\"aggregatable\":true,\"readFromDocValues\":false}]"},"type":"index-pattern","updated_at":"2019-07-16T16:19:23.016Z","migrationVersion":{"index-pattern":"6.5.0"}},"fields":null,"sort":null,"_seq_no":55,"_primary_term":1}]}
*/
