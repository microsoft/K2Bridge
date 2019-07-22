namespace K2Bridge
{
    using System.Collections.Generic;
    using Newtonsoft.Json;

    internal class QueryTranslator
    {
        private readonly ElasticSearchDSLVisitor visitor = new ElasticSearchDSLVisitor();

        public string Translate(string header, string query)
        {
            var headerDictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(header);

            var elasticSearchDSL = JsonConvert.DeserializeObject<ElasticSearchDSL>(query);
            elasticSearchDSL.IndexName = headerDictionary["index"];

            elasticSearchDSL.Accept(visitor);
            return elasticSearchDSL.KQL;
        }
    }
}
