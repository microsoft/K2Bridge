namespace K2Bridge.Models.Response
{
    using System.Data;
    using K2Bridge.KustoConnector;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    public class Hit
    {
        private const string ID = "999"; // arbitrary decided on this value. used internally by kibana
        private const string TYPE = "_doc";
        private const int VERSION = 1;

        [JsonProperty("_index")]
        public string Index { get; set; }

        [JsonProperty("_type")]
        public string Type { get; } = TYPE;

        [JsonProperty("_id")]
        public string Id { get; } = ID;

        [JsonProperty("_version")]
        public int Version { get; set; } = VERSION;

        [JsonProperty("_score")]
        public object Score { get; set; }

        [JsonProperty("_source")]
        public JObject Source { get; } = new JObject();

        [JsonProperty("fields", NullValueHandling = NullValueHandling.Ignore)]
        public Fields Fields { get; set; }

        [JsonProperty("sort", NullValueHandling = NullValueHandling.Ignore)]
        public long[] Sort { get; set; }

        public void AddSource(string keyName, object value)
        {
            this.Source.Add(keyName, value == null ? null : JToken.FromObject(value));
        }

        public static Hit Create(IDataRecord record, string indexName)
        {
            var hit = new Hit() { Index = indexName };
            for (int index = 0; index < record.FieldCount; index++)
            {
                var name = record.GetName(index);
                var value = record.ReadValue(index);
                hit.AddSource(name, value);
            }

            return hit;
        }
    }
}
