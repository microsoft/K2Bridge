// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2Bridge.Models.Response
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using K2Bridge.Models;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    public class Hit
    {
        private const string TYPE = "_doc";
        private const int VERSION = 1;

        private static readonly Dictionary<Type, Func<object, object>> Converters = new Dictionary<Type, Func<object, object>>
        {
            { typeof(sbyte), (value) => (sbyte)value != 0 },

            // Elasticsearch returns timestamp fields in UTC in ISO-8601 but without Timezone.
            // Use a String type to control serialization to mimic this behavior.
            { typeof(DateTime), (value) => value != null ? ((DateTime)value).ToString("yyyy-MM-dd'T'HH:mm:ss.FFFFFFF") : null },
        };

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

        [JsonProperty("sort", NullValueHandling = NullValueHandling.Ignore)]
        public long[] Sort { get; set; }

        [JsonProperty("highlight", NullValueHandling = NullValueHandling.Ignore)]
        public Dictionary<string, object> Highlight { get; set; }

        public static Hit Create(DataRow row, QueryData query)
        {
            Ensure.IsNotNull(row, nameof(row));

            var hit = new Hit() { Index = query.IndexName };
            hit.Highlight = new Dictionary<string, object>();

            var columns = row.Table.Columns;

            for (int index = 0; index < row.ItemArray.Length; index++)
            {
                var name = columns[index].ColumnName;
                var value = GetValue(columns[index], row[name]);
                hit.AddSource(name, value);

                if (query.HighlightText == null || value == null)
                {
                    continue;
                }

                // Elastic only highlights string values, but we try to highlight everything we can here.
                // To mimic elastic: check for type of value here and skip if != string.
                // HighlightText.ContainsKey(name) condition will be true when searching with the available filters
                // HighlightText.ContainsKey("*") condition will be true when searching with the search box
                if (query.HighlightText.ContainsKey(name) && value.ToString().Equals(query.HighlightText[name], StringComparison.OrdinalIgnoreCase))
                {
                    hit.Highlight.Add(name, new List<string> { query.HighlightPreTag + query.HighlightText[name] + query.HighlightPostTag });
                }
                else if (query.HighlightText.ContainsKey("*") && value.ToString().Contains(query.HighlightText["*"], StringComparison.OrdinalIgnoreCase))
                {
                    hit.Highlight.Add(name, new List<string> { query.HighlightPreTag + query.HighlightText["*"] + query.HighlightPostTag });
                }
            }

            return hit;
        }

        public void AddSource(string keyName, object value)
        {
            Source.Add(keyName, value == null ? null : JToken.FromObject(value));
        }

        private static object GetValue(DataColumn column, object value)
        {
            if (value == null)
            {
                return null;
            }

            var type = column.DataType;
            if (Converters.ContainsKey(type))
            {
                return Converters[type](value);
            }

            return value;
        }
    }
}
