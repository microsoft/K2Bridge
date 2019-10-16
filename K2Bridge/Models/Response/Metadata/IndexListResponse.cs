namespace K2Bridge.Models.Response.Metadata
{
    using Newtonsoft.Json;

    internal class IndexListResponse
    {
        private const int TOOK = 1;
        private const int STATUS = 200;

        [JsonProperty("took")]
        public int Took { get; set; } = TOOK;

        [JsonProperty("timed_out")]
        public bool TimedOut { get; set; }

        [JsonProperty("_shards")]
        public Shards Shards { get; set; } = new Shards();

        [JsonProperty("hits")]
        public HitsCollection Hits { get; set; } = new HitsCollection();

        [JsonProperty("aggregations")]
        public IndexListAggregations Aggregations { get; set; } = new IndexListAggregations();

        [JsonProperty("status")]
        public int Status { get; set; } = STATUS;
    }
}
