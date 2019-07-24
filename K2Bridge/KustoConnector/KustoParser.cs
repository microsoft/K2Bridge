using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace K2Bridge.KustoConnector
{
    public class KustoParser
    {
        public static void ReadHits(IDataReader reader, ElasticResponse response)
        {
            List<Hit> hits = new List<Hit>();

            while (reader.Read())
            {
                var hit = JsonConvert.DeserializeObject<Hit>(reader.GetValue(0).ToString());
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
