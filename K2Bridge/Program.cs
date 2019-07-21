namespace K2Bridge
{
    using System;
    using System.IO;
    using Newtonsoft.Json;

    public class Program
    {
        public static void Main(string[] args)
        {
            string proxyAddress = @"http://localhost:8080/";
            string elasticAddress = @"http://localhost:9200";

            var translator = new QueryTranslator();

            SimpleListener.Start(new string[] { proxyAddress }, elasticAddress, translator);
        }
    }
}
