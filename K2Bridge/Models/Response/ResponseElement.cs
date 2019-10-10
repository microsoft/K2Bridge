namespace K2Bridge.Models.Response
{
    using Newtonsoft.Json;

    public class ResponseElement
    {
        const int TOOK = 1;
        const int STATUS = 200;

        [JsonProperty("took")]
        public int Took { get; set; } = TOOK;

        [JsonProperty("timed_out")]
        public bool TimedOut { get; set; }

        [JsonProperty("_shards")]
        public Shards Shards { get; set; } = new Shards();

        [JsonProperty("hits")]
        public HitsCollection Hits { get; set; } = new HitsCollection();

        [JsonProperty("aggregations")]
        public Aggregations Aggregations { get; set; } = new Aggregations();

        [JsonProperty("status")]
        public int Status { get; set; } = STATUS;
    }
}
