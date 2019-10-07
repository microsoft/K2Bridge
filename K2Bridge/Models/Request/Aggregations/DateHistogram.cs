namespace K2Bridge.Models.Request.Aggregations
{
    using Newtonsoft.Json;

    [JsonConverter(typeof(DateHistogramConverter))]
    internal class DateHistogram : BucketAggregation
    {
        [JsonProperty("field")]
        public string FieldName { get; set; }

        [JsonProperty("interval")]
        public string Interval { get; set; }

        public override void Accept(IVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
}
