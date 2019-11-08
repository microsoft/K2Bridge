namespace K2Bridge.Models.Response
{
    using Newtonsoft.Json;

    public class ResponseElement : ResponseElementBase
    {
        [JsonProperty("aggregations")]
        public Aggregations Aggregations { get; set; } = new Aggregations();
    }
}
