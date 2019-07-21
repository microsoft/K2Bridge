namespace K2Bridge
{
    using Newtonsoft.Json;

    [JsonConverter(typeof(RangeQueryConverter))]
    internal class RangeQuery : LeafQueryClause, IVisitable
    {
        public string FieldName { get; set; }

        public long GTEValue { get; set; }

        public long LTEValue { get; set; }

        public void Accept(IVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
}
