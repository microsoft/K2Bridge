namespace K2Bridge
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Text;
    using System.Data;
    using K2Bridge.KustoConnector;
    using Newtonsoft.Json;

    internal class IndexListRequestHandler: RequestHandler
    {
        public IndexListRequestHandler(HttpListenerContext requestContext, KustoManager kustoClient, Guid requestId)
            :base()
        {
            this.context = requestContext;

            this.kusto = kustoClient;
    }

    public static bool Mine(string rawUrl, string requestString)
        {
            string requestSignature = "[{\"term\":{\"type\":\"index-pattern\"}}]";
            //debug hack
            if (!rawUrl.StartsWith(@"/.kibana/_search"))
                return false;

            return
                rawUrl.StartsWith(@"/.kibana/_search") &&
                (-1 != requestString.IndexOf(requestSignature));
        }

        public HttpListenerResponse PrepareResponse(string requestInputString)
        {
            try
            {
                IDataReader kustoResults = this.kusto.ExecuteControlCommand(".show tables");

                string[] tables = new string[0];
                int i = 0;

                while (kustoResults.Read())
                {
                    IDataRecord record = (IDataRecord)kustoResults;

                    tables.Append(record[0].ToString());

                    this.Logger.Debug($"Found index: {record[0].ToString()}");
                }

                this.Logger.Information($"Request index list: {tables.ToString()}");

                kustoResults.Close();
            }
            catch (Exception ex)
            {
                this.Logger.Debug($"Failed to retrieve indexes");
            }

            KustoConnector.Response elasticOutputStream = JsonConvert.DeserializeObject< KustoConnector.Response> (
                               "{\"took\":0,\"timed_out\":false,\"_shards\":{\"total\":1,\"successful\":1,\"skipped\":0,\"failed\":0},\"hits\":{\"total\":3,\"max_score\":0.0,\"hits\":[{\"_index\":\".kibana_1\",\"_type\":\"doc\",\"_id\":\"index-pattern:90943e30-9a47-11e8-b64d-95841ca0b247\",\"_seq_no\":55,\"_primary_term\":1,\"_score\":0.0,\"_source\":{\"index-pattern\":{\"title\":\"kibana_sample_data_logs\"},\"type\":\"index-pattern\"}},{\"_index\":\".kibana_1\",\"_type\":\"doc\",\"_id\":\"index-pattern:ff959d40-b880-11e8-a6d9-e546fe2bba5f\",\"_seq_no\":65,\"_primary_term\":2,\"_score\":0.0,\"_source\":{\"index-pattern\":{\"title\":\"kibana_sample_data_ecommerce\"},\"type\":\"index-pattern\"}},{\"_index\":\".kibana_1\",\"_type\":\"doc\",\"_id\":\"index-pattern:d3d7af60-4c81-11e8-b3d7-01146121b73d\",\"_seq_no\":67,\"_primary_term\":2,\"_score\":0.0,\"_source\":{\"index-pattern\":{\"title\":\"kibana_sample_data_flights\"},\"type\":\"index-pattern\"}}]}}");

            //Responses.took

            HttpListenerResponse response = this.context.Response;

            string kustoResultsString = JsonConvert.SerializeObject(elasticOutputStream);

            this.Logger.Debug($"Index list output:{kustoResultsString}");

            byte[] kustoResultsContent = Encoding.ASCII.GetBytes(kustoResultsString);

            var kustoResultsStream = new MemoryStream(kustoResultsContent);

            response.StatusCode = 200;
            response.ContentLength64 = kustoResultsContent.LongLength;
            response.ContentType = "application/json";
            kustoResultsStream.CopyTo(response.OutputStream);
            response.OutputStream.Close();

            return response;
        }
    }
}
