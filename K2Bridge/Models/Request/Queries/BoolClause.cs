namespace K2Bridge.Models.Request.Queries
{
    using System.Collections.Generic;
    using K2Bridge.Models.Request;
    using K2Bridge.Visitors;
    using Newtonsoft.Json;

    internal class BoolClause : KQLBase, IVisitable
    {
        [JsonProperty("must")]
        public List<LeafQueryClause> Must { get; set; }

        [JsonProperty("must_not")]
        public List<LeafQueryClause> MustNot { get; set; }

        public void Accept(IVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
}
