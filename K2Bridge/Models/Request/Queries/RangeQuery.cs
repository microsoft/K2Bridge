namespace K2Bridge.Models.Request.Queries
{
    using K2Bridge.Models.Request;
    using K2Bridge.Visitors;
    using Newtonsoft.Json;

    [JsonConverter(typeof(RangeQueryConverter))]
    internal class RangeQuery : KQLBase, ILeafQueryClause, IVisitable
    {
        public string FieldName { get; set; }

        public decimal? GTEValue { get; set; }

        public decimal? GTValue { get; set; } // isn't created by kibana but kept here for completeness

        public decimal? LTEValue { get; set; }

        public decimal? LTValue { get; set; }

        public string Format { get; set; }

        public void Accept(IVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
}
