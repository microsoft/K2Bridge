namespace K2Bridge.Models.Request.Aggregations
{
    using Newtonsoft.Json;

    [JsonConverter(typeof(LeafAggregationConverter))]
    internal abstract class LeafAggregation : KQLBase, IVisitable
    {
        public abstract void Accept(IVisitor visitor);
    }
}
