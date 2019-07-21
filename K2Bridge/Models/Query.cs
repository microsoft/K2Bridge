namespace K2Bridge
{
    using Newtonsoft.Json;

    internal class Query : KQLBase, IVisitable
    {
        [JsonProperty("bool")]
        public BoolClause Bool { get; set; }

        public void Accept(IVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
}
