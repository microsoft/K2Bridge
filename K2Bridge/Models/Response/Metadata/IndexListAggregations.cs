namespace K2Bridge.Models.Response.Metadata
{
    using Newtonsoft.Json;

    internal class IndexListAggregations
    {
        [JsonProperty("indices")]
        public BucketsCollection IndicesCollection { get; set; } = new BucketsCollection();
    }
}
