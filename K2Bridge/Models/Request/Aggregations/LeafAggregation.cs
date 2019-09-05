namespace K2Bridge.Models.Aggregations
{
    using Newtonsoft.Json;

    [JsonConverter(typeof(LeafAggregationConverter))]
    internal abstract class LeafAggregation : KQLBase, IVisitable
    {
        public abstract void Accept(IVisitor visitor);
    }
}
