namespace K2Bridge
{
    using System;
    using System.Linq;
    using System.Net;
    using System.Data;
    using System.Collections.Generic;
    using Newtonsoft.Json;
    using K2Bridge.KustoConnector;


    internal class IndexListRequestHandler: RequestHandler
    {
        public static bool Mine(string rawUrl, string requestString)
        {
            string requestSignature = "[ { \"term\": { \"type\": \"index-pattern\" } } ]";

            return
                (rawUrl.StartsWith(@"/.kibana/_search") &&
                (-1 != requestString.IndexOf(requestSignature)));
        }

        HttpListenerContext context;
        KustoManager        kusto;
        public Serilog.ILogger Logger { get; set; }

        public IndexListRequestHandler(HttpListenerContext requestContext, KustoManager kustoClient, Guid requestId)
        {
            context = requestContext;

            kusto = kustoClient;
        }

        public HttpListenerResponse PrepareResponse(string requestInputString)
        {
            try
            {
                IDataReader kustoResults = kusto.ExecuteControlCommand(".show tables");

                string[] tables = new string[0];
                int i = 0;

                while (kustoResults.Read())
                {
                    IDataRecord record = (IDataRecord)kustoResults;

                    tables.Append(record[i++].ToString());
                }

                this.Logger.Information($"Request index list: {tables.ToString()}");
            }
            catch (Exception ex)
            {
                this.Logger.Debug($"Failed to retrieve indexes");
            }

            HttpListenerResponse response = context.Response;

            return response;
        }
    }
}
