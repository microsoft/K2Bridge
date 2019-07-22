namespace K2Bridge
{
    using System.Collections.Generic;
    using Newtonsoft.Json;

    internal class BoolClause : KQLBase, IVisitable
    {
        [JsonProperty("must")]
        public List<LeafQueryClause> Must { get; set; }

        public void Accept(IVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
}
