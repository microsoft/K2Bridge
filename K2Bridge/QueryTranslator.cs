namespace K2Bridge
{
    using Newtonsoft.Json;

    internal class QueryTranslator
    {
        private readonly ElasticSearchDSLVisitor visitor = new ElasticSearchDSLVisitor();

        public string Translate(string query)
        {
            var elasticSearchDSL = JsonConvert.DeserializeObject<ElasticSearchDSL>(query);
            elasticSearchDSL.Accept(this.visitor);
            return elasticSearchDSL.KQL;
        }
    }
}
