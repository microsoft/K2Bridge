// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2Bridge.Models.Response;

using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

/// <summary>
/// Single search hit.
/// </summary>
public class Hit
{
    private const string TYPE = "_doc";
    private const int VERSION = 1;

    /// <summary>
    /// Gets or sets index name.
    /// </summary>
    [JsonProperty("_index")]
    public string Index { get; set; }

    /// <summary>
    /// Gets hit type.
    /// </summary>
    [JsonProperty("_type")]
    public string Type { get; } = TYPE;

    /// <summary>
    /// Gets or sets the doc id.
    /// </summary>
    [JsonProperty("_id")]
    public string Id { get; set; }

    /// <summary>
    /// Gets or sets the hit version.
    /// </summary>
    [JsonProperty("_version")]
    public int Version { get; set; } = VERSION;

    /// <summary>
    /// Gets or sets the hit score.
    /// </summary>
    [JsonProperty("_score")]
    public object Score { get; set; }

    /// <summary>
    /// Gets the source.
    /// </summary>
    [JsonProperty("_source")]
    public JObject Source { get; } = new JObject();

    /// <summary>
    /// Gets the fields asked by the client in the doc_fields element.
    /// Apparently, the fields are used by the client to convert date to the proper timezone.
    /// </summary>
    [JsonProperty("fields")]
    public Dictionary<string, List<object>> Fields { get; } = new Dictionary<string, List<object>>();

    /// <summary>
    /// Gets the sort objects.
    /// </summary>
    [JsonProperty("sort")]
    public IList<object> Sort { get; } = new List<object>();

    [JsonProperty("highlight", NullValueHandling = NullValueHandling.Ignore)]
    public Dictionary<string, object> Highlight { get; private set; }

    /// <summary>
    /// Add source to hit response.
    /// </summary>
    /// <param name="keyName">The key name.</param>
    /// <param name="value">The key value.</param>
    public void AddSource(string keyName, object value)
    {
        Source.Add(keyName, value == null ? null : JToken.FromObject(value));
    }

    /// <summary>
    /// Add column highlight to hit response.
    /// </summary>
    /// <param name="columnName">The column name.</param>
    /// <param name="value">The column value.</param>
    public void AddColumnHighlight(string columnName, object value)
    {
        if (Highlight == null)
        {
            Highlight = new Dictionary<string, object>();
        }

        Highlight.Add(columnName, value);
    }
}
