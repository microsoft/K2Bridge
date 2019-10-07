namespace K2Bridge.Models.Request.Aggregations
{
    using Newtonsoft.Json;

    [JsonConverter(typeof(JustFieldConverter))]
    internal class Avg : MetricAggregation
    {
        [JsonProperty("field")]
        public string FieldName { get; set; }

        public override void Accept(IVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
}
