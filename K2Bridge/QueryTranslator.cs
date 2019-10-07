namespace K2Bridge
{
    using System.Collections.Generic;
    using Newtonsoft.Json;

    internal class QueryTranslator : ITranslator
    {
        private readonly IVisitor visitor;

        public QueryTranslator(IVisitor visitor) => this.visitor = visitor;

        public string Translate(string header, string query)
        {
            var headerDictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(header);

            var elasticSearchDSL = JsonConvert.DeserializeObject<ElasticSearchDSL>(query);
            elasticSearchDSL.IndexName = headerDictionary["index"];

            elasticSearchDSL.Accept(this.visitor);
            return elasticSearchDSL.KQL;
        }
    }
}
