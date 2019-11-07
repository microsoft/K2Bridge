namespace K2Bridge.Models.Request.Queries
{
    using System.Collections.Generic;
    using Newtonsoft.Json;

    internal class Highlight
    {
        [JsonProperty("pre_tags")]
        public List<string> PreTags { get; set; }

        [JsonProperty("post_tags")]
        public List<string> PostTags { get; set; }
    }
}
