namespace K2Bridge.Models.Aggregations
{
    internal abstract class BucketAggregation : LeafAggregation
    {
        public string Metric { get; set; } = "count()";
    }
}
