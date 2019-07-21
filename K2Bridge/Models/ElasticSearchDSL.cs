namespace K2Bridge
{
    using System.Collections.Generic;
    using Newtonsoft.Json;

    internal class ElasticSearchDSL : KQLBase, IVisitable
    {
        [JsonProperty("query")]
        public Query Query { get; set; }

        [JsonProperty("size")]
        public int Size { get; set; }

        [JsonProperty("timeout")]
        public string Timeout { get; set; }

        [JsonProperty("sort")]
        public List<SortClause> Sort { get; set; }

        public void Accept(IVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
}
