using System;
using System.Collections.Generic;
using System.Text;

namespace K2Bridge.KustoConnector
{
    using System.Data;
    using Kusto.Data;
    using Kusto.Data.Common;
    using Kusto.Data.Net.Client;

    public class KustoManager
    {
        private ICslQueryProvider client;

        public KustoManager()
        {
            KustoConnectionStringBuilder conn = new KustoConnectionStringBuilder("https://kustolab.kusto.windows.net/takamara;Fed=true")
                .WithAadApplicationKeyAuthentication("<App>", "<Key>", "Authority");

            this.client = KustoClientFactory.CreateCslQueryProvider(conn);

            // this.client = KustoClientFactory.CreateCslQueryProvider("https://kustolab.kusto.windows.net;Initial Catalog=takamara;Fed=true");
            this.client.DefaultDatabaseName = "takamara";
        }

        public IDataReader ExecuteQuery(string query)
        {
            // var result = this.client.ExecuteQuery("kibana_sample_data_flights | take 10");
            IDataReader result = this.client.ExecuteQuery(query);
            Console.WriteLine("columns received = " + result.FieldCount);

            return result;
        }
    }
}
