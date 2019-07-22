namespace K2Bridge.Models.Aggregations
{
    using Newtonsoft.Json;

    //[JsonConverter(typeof(DateHistogramConverter))]
    internal class Avg : MetricAggregation
    {
        [JsonProperty("field")]
        public string FieldName { get; set; }

        public override void Accept(IVisitor visitor)
        {
            //visitor.Visit(this);
        }
    }
}
