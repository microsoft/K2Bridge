namespace K2Bridge.Models.Request
{
    using System.Collections.Generic;
    using K2Bridge.Models.Request.Aggregations;
    using K2Bridge.Models.Request.Queries;
    using K2Bridge.Visitors;
    using Newtonsoft.Json;

    internal class ElasticSearchDSL : KQLBase, IVisitable
    {
        [JsonProperty("query")]
        public Query Query { get; set; }

        [JsonProperty("size")]
        public int Size { get; set; }

        /*
        [JsonProperty("timeout")]
        public string Timeout { get; set; }
        */

        [JsonProperty("sort")]
        public List<SortClause> Sort { get; set; }

        [JsonProperty("aggs")]
        public Dictionary<string, Aggregation> Aggregations { get; set; }

        [JsonProperty("highlight")]
        public Highlight Highlight { get; set; }

        public Dictionary<string, string> HighlightText { get; set; }

        public string IndexName { get; set; }

        public void Accept(IVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
}
