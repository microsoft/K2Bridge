namespace K2Bridge
{
    using Newtonsoft.Json;

    class ElasticSearchDSL : KQLBase, IVisitable
    {
        [JsonProperty("query")]
        public Query Query { get; set; }

        public void Accept(IVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
}
