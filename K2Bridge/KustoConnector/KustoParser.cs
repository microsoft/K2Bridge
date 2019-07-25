namespace K2Bridge.KustoConnector
{
    using System.Collections.Generic;
    using System.Data;
    using Newtonsoft.Json.Linq;

    public class KustoParser
    {
        public static void ReadHits(IDataReader reader, ElasticResponse response)
        {
            List<Hit> hits = new List<Hit>();

            while (reader.Read())
            {
                var jo = (JObject)reader.GetValue(0);
                var hit = jo.ToObject<Hit>();
                hits.Add(hit);
            }

            response.responses[0].hits.total = hits.Count;
            response.responses[0].hits.hits = hits.ToArray();
        }

        public static void ReadAggs(IDataReader reader, ElasticResponse response)
        {

        }
    }
}
