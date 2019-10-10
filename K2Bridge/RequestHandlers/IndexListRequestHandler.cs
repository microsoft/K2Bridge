namespace K2Bridge.RequestHandlers
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Net;
    using System.Text;
    using K2Bridge.KustoConnector;
    using K2Bridge.Models.Response.Metadata;
    using Microsoft.Extensions.Logging;
    using Newtonsoft.Json;

    internal class IndexListRequestHandler : KibanaRequestHandler
    {
        public IndexListRequestHandler(HttpListenerContext requestContext, IQueryExecutor kustoClient, string requestId, ILogger logger)
            : base(requestContext, kustoClient, requestId, logger)
        {
        }

        public static bool Mine(string rawUrl, string requestString)
        {
            string requestSignature = "[{\"term\":{\"type\":\"index-pattern\"}}]";

            /*
            .kibana/_search?size=10000&from=0&_source=index-pattern.title%2Cindex-pattern.type%2Cnamespace%2Ctype%2Ctitle%2Ctype&rest_total_hits_as_int=true
            .kibana/_search?size=10000&from=0&_source=index-pattern.title%2Cnamespace                     %2Ctype%2Ctitle       &rest_total_hits_as_int=true
            */

            return
                rawUrl.StartsWith(@"/.kibana/_search?size=10000&from=0&_source=index-pattern") &&
                requestString.IndexOf(requestSignature) != -1;
        }

        public string PrepareResponse(string requestInputString)
        {
            try
            {
                StringBuilder sb = new StringBuilder();

                IDataReader kustoResults = this.kusto.ExecuteControlCommand(".show tables");

                Response elasticOutputStream = JsonConvert.DeserializeObject<Response>(
                                   "{\"took\":0,\"timed_out\":false,\"_shards\":{\"total\":1,\"successful\":1,\"skipped\":0,\"failed\":0},\"hits\":{\"total\":3,\"max_score\":0.0,\"hits\":[{\"_index\":\".kibana_1\",\"_type\":\"doc\",\"_id\":\"index-pattern:90943e30-9a47-11e8-b64d-95841ca0b247\",\"_seq_no\":55,\"_primary_term\":1,\"_score\":0.0,\"_source\":{\"index-pattern\":{\"title\":\"kibana_sample_data_logs\"},\"type\":\"index-pattern\"}},{\"_index\":\".kibana_1\",\"_type\":\"doc\",\"_id\":\"index-pattern:ff959d40-b880-11e8-a6d9-e546fe2bba5f\",\"_seq_no\":65,\"_primary_term\":2,\"_score\":0.0,\"_source\":{\"index-pattern\":{\"title\":\"kibana_sample_data_ecommerce\"},\"type\":\"index-pattern\"}},{\"_index\":\".kibana_1\",\"_type\":\"doc\",\"_id\":\"index-pattern:d3d7af60-4c81-11e8-b3d7-01146121b73d\",\"_seq_no\":67,\"_primary_term\":2,\"_score\":0.0,\"_source\":{\"index-pattern\":{\"title\":\"kibana_sample_data_flights\"},\"type\":\"index-pattern\"}}]}}");

                elasticOutputStream.took = 10; // TODO: need to add the stats from the actual query.

                Hits hitsField = new Hits();

                List<Hit> hitsList = new List<Hit>();

                while (kustoResults.Read())
                {
                    IDataRecord record = kustoResults;

                    sb.Append(record[0].ToString());
                    sb.Append(", ");

                    Source source = new Source();

                    Hit hit = new Hit();
                    hit._source = source;
                    hit._source.index_pattern = new IndexPattern();

                    string tableName = record[0].ToString();
                    string indexID = this.GetIndexGuid(tableName).ToString();

                    hit._index = ".kibana_1";
                    hit._type = "doc";
                    hit._id = $"index-pattern:{indexID}";
                    hit._seq_no = 55;
                    hit._primary_term = 1;
                    hit._score = 0.0;
                    source.index_pattern.title = tableName;
                    source.type = "index-pattern";

                    hitsList.Add(hit);

                    this.logger.LogDebug($"Found index: {record[0].ToString()}");
                }

                kustoResults.Close();

                elasticOutputStream.hits.total = hitsList.Count;
                elasticOutputStream.hits.hits = hitsList.ToArray();

                HttpListenerResponse response = this.context.Response;

                string kustoResultsString = JsonConvert.SerializeObject(elasticOutputStream);

                return kustoResultsString;
            }
            catch (Exception)
            {
                this.logger.LogDebug($"Failed to retrieve indexes");

                throw;
            }
        }
    }
}

/*
URL
/.kibana/_search?size=10000&from=0&_source=index-pattern.title%2Cnamespace%2Ctype%2Ctitle&rest_total_hits_as_int=true
Request:
{"seq_no_primary_term":true,"query":{"bool":{"filter":[{"bool":{"should":[{"bool":{"must":[{"term":{"type":"index-pattern"}}],"must_not":[{"exists":{"field":"namespace"}}]}}],"minimum_should_match":1}}]}}}
Elastic Response
{"took":0,"timed_out":false,"_shards":{"total":1,"successful":1,"skipped":0,"failed":0},"hits":{"total":3,"max_score":0.0,"hits":[{"_index":".kibana_1","_type":"doc","_id":"index-pattern:d3d7af60-4c81-11e8-b3d7-01146121b73d","_seq_no":67,"_primary_term":2,"_score":0.0,"_source":{"index-pattern":{"title":"kibana_sample_data_flights"},"type":"index-pattern"}},{"_index":".kibana_1","_type":"doc","_id":"index-pattern:90943e30-9a47-11e8-b64d-95841ca0b247","_seq_no":94,"_primary_term":2,"_score":0.0,"_source":{"index-pattern":{"title":"kibana_sample_data_logs"},"type":"index-pattern"}},{"_index":".kibana_1","_type":"doc","_id":"index-pattern:b0907530-b109-11e9-954c-116694d05331","_seq_no":106,"_primary_term":2,"_score":0.0,"_source":{"index-pattern":{"title":"kibana_sample_data_ecommerce"},"type":"index-pattern"}}]}}
K2 current Response:
{"took":10,"timed_out":false,"_shards":{"total":1,"successful":1,"skipped":0,"failed":0},"hits":{"total":10,"max_score":0.0,"hits":[{"_index":".kibana_1","_type":"doc","_id":"index-pattern:8b5e105a-409d-32e1-9780-d62ea2265d8a","_version":0,"_score":0.0,"_source":{"index-pattern":{"title":"test1","timeFieldName":null,"fieldFormatMap":null,"fields":null},"type":"index-pattern","updated_at":null,"migrationVersion":null},"fields":null,"sort":null,"_seq_no":55,"_primary_term":1},{"_index":".kibana_1","_type":"doc","_id":"index-pattern:d44db785-56f6-0b30-c18e-0163e1a1a858","_version":0,"_score":0.0,"_source":{"index-pattern":{"title":"pos_data","timeFieldName":null,"fieldFormatMap":null,"fields":null},"type":"index-pattern","updated_at":null,"migrationVersion":null},"fields":null,"sort":null,"_seq_no":55,"_primary_term":1},{"_index":".kibana_1","_type":"doc","_id":"index-pattern:cde8e481-3bc6-1c4a-625a-82c25f9d76da","_version":0,"_score":0.0,"_source":{"index-pattern":{"title":"logstash1","timeFieldName":null,"fieldFormatMap":null,"fields":null},"type":"index-pattern","updated_at":null,"migrationVersion":null},"fields":null,"sort":null,"_seq_no":55,"_primary_term":1},{"_index":".kibana_1","_type":"doc","_id":"index-pattern:6f059db8-09bc-396f-56b2-f8984992b17a","_version":0,"_score":0.0,"_source":{"index-pattern":{"title":"TempTable","timeFieldName":null,"fieldFormatMap":null,"fields":null},"type":"index-pattern","updated_at":null,"migrationVersion":null},"fields":null,"sort":null,"_seq_no":55,"_primary_term":1},{"_index":".kibana_1","_type":"doc","_id":"index-pattern:8abafdd0-85fd-3188-11fd-dffbc645acee","_version":0,"_score":0.0,"_source":{"index-pattern":{"title":"myTable","timeFieldName":null,"fieldFormatMap":null,"fields":null},"type":"index-pattern","updated_at":null,"migrationVersion":null},"fields":null,"sort":null,"_seq_no":55,"_primary_term":1},{"_index":".kibana_1","_type":"doc","_id":"index-pattern:1d6026ce-0dac-13ea-8b72-95f02b7620a7","_version":0,"_score":0.0,"_source":{"index-pattern":{"title":"Customer","timeFieldName":null,"fieldFormatMap":null,"fields":null},"type":"index-pattern","updated_at":null,"migrationVersion":null},"fields":null,"sort":null,"_seq_no":55,"_primary_term":1},{"_index":".kibana_1","_type":"doc","_id":"index-pattern:90943e30-9a47-11e8-b64d-95841ca0b247","_version":0,"_score":0.0,"_source":{"index-pattern":{"title":"kibana_sample_data_logs","timeFieldName":null,"fieldFormatMap":null,"fields":null},"type":"index-pattern","updated_at":null,"migrationVersion":null},"fields":null,"sort":null,"_seq_no":55,"_primary_term":1},{"_index":".kibana_1","_type":"doc","_id":"index-pattern:d3d7af60-4c81-11e8-b3d7-01146121b73d","_version":0,"_score":0.0,"_source":{"index-pattern":{"title":"kibana_sample_data_flights","timeFieldName":null,"fieldFormatMap":null,"fields":null},"type":"index-pattern","updated_at":null,"migrationVersion":null},"fields":null,"sort":null,"_seq_no":55,"_primary_term":1},{"_index":".kibana_1","_type":"doc","_id":"index-pattern:ff959d40-b880-11e8-a6d9-e546fe2bba5f","_version":0,"_score":0.0,"_source":{"index-pattern":{"title":"kibana_sample_data_ecommerce","timeFieldName":null,"fieldFormatMap":null,"fields":null},"type":"index-pattern","updated_at":null,"migrationVersion":null},"fields":null,"sort":null,"_seq_no":55,"_primary_term":1},{"_index":".kibana_1","_type":"doc","_id":"index-pattern:2e954a28-86fb-c87f-712d-560dbf094ab8","_version":0,"_score":0.0,"_source":{"index-pattern":{"title":"kibana_sample_data_flights_fixed","timeFieldName":null,"fieldFormatMap":null,"fields":null},"type":"index-pattern","updated_at":null,"migrationVersion":null},"fields":null,"sort":null,"_seq_no":55,"_primary_term":1}]},"aggregations":null,"status":0}
*/
