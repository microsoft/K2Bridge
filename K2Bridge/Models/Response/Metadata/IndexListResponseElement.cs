namespace K2Bridge.Models.Response.Metadata
{
    using Newtonsoft.Json;

    internal class IndexListResponseElement : ResponseElementBase
    {
        [JsonProperty("aggregations")]
        public IndexListAggregations Aggregations { get; set; } = new IndexListAggregations();
    }
}