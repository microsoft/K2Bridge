namespace K2Bridge.Models.Aggregations
{
    using System.Collections.Generic;
    using Newtonsoft.Json;

    [JsonConverter(typeof(AggregationConverter))]
    internal class Aggregation : KQLBase, IVisitable
    {
        public LeafAggregation PrimaryAggregation { get; set; }

        public Dictionary<string, Aggregation> SubAggregations { get; set; }

        public void Accept(IVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
}
