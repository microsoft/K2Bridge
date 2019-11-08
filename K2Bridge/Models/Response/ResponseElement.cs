namespace K2Bridge.Models.Response
{
    using Newtonsoft.Json;

    public class ResponseElement
    {
        private const int STATUS = 200;

        [JsonProperty("took")]
        public long TookMilliseconds { get; set; }

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
