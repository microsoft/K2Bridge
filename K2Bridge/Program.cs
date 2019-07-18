namespace K2Bridge
{
    using System;
    using System.IO;
    using Newtonsoft.Json;

    class Program
    {
        static void Main(string[] args)
        {
            ElasticSearchDSL elasticSearchDSL = JsonConvert.DeserializeObject<ElasticSearchDSL>(File.ReadAllText(@"C:\dev\kusto\esproxy\src\main\resources\query2.json"));

            ElasticSearchDSLVisitor visitor = new ElasticSearchDSLVisitor();
            elasticSearchDSL.Accept(visitor);

            Console.WriteLine(elasticSearchDSL.KQL);
        }
    }
}
