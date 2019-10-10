namespace K2Bridge
{
    using System.Collections.Generic;
    using K2Bridge.Models.Request;
    using K2Bridge.Visitors;
    using Newtonsoft.Json;

    internal class QueryTranslator : ITranslator
    {
        private readonly IVisitor visitor;

        public QueryTranslator(IVisitor visitor) => this.visitor = visitor;

        public QueryData Translate(string header, string query)
        {
            var headerDictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(header);

            var elasticSearchDSL = JsonConvert.DeserializeObject<ElasticSearchDSL>(query);
            elasticSearchDSL.IndexName = headerDictionary["index"];

            elasticSearchDSL.Accept(this.visitor);
            return new QueryData(elasticSearchDSL.KQL, elasticSearchDSL.IndexName);
        }
    }
}
