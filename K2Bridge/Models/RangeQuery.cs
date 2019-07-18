namespace K2Bridge
{
    using Newtonsoft.Json;

    [JsonConverter(typeof(RangeQueryConverter))]
    class RangeQuery : LeafQueryClause, IVisitable
    {
        public string FieldName { get; set; }

        public int GTEValue { get; set; }

        public int LTEValue { get; set; }

        public void Accept(IVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
}
