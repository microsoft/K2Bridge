// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2Bridge.Models.Response
{
    using System.Collections.Generic;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    public class Hit
    {
        private const string TYPE = "_doc";
        private const int VERSION = 1;

        [JsonProperty("_index")]
        public string Index { get; set; }

        [JsonProperty("_type")]
        public string Type { get; } = TYPE;

        [JsonProperty("_id")]
        public string Id { get; set; }

        [JsonProperty("_version")]
        public int Version { get; set; } = VERSION;

        [JsonProperty("_score")]
        public object Score { get; set; }

        [JsonProperty("_source")]
        public JObject Source { get; } = new JObject();

        [JsonProperty("fields", NullValueHandling = NullValueHandling.Ignore)]
        public Fields Fields { get; set; }

        [JsonProperty("sort")]
        public IList<object> Sort { get; } = new List<object>();

        [JsonProperty("highlight", NullValueHandling = NullValueHandling.Ignore)]
        public Dictionary<string, object> Highlight { get; private set; }

        public static Hit Create(string id, string indexName)
        => new Hit() { Id = id, Index = indexName };

        public void AddSource(string keyName, object value)
        {
            Source.Add(keyName, value == null ? null : JToken.FromObject(value));
        }

        public void AddColumnHighlight(string columnName, object value)
        {
            if (Highlight == null)
            {
                Highlight = new Dictionary<string, object>();
            }

            Highlight.Add(columnName, value);
        }
    }
}