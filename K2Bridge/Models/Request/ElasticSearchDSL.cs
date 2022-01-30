// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2Bridge.Models.Request;

using System.Collections.Generic;
using K2Bridge.Models.Request.Aggregations;
using K2Bridge.Models.Request.Queries;
using K2Bridge.Visitors;
using Newtonsoft.Json;

/// <summary>
/// ElasticSearchDSL (Elasticsearch Domain Specific Language) represents
/// the different properties of the elasticsearch query as deserialized from
/// the json object sent from Kibana. This object will be sent for transformation.
/// </summary>
internal class ElasticSearchDSL : KustoQLBase, IVisitable
{
    /// <summary>
    /// Gets or sets the query object.
    /// </summary>
    [JsonProperty("query")]
    public Query Query { get; set; }

    /// <summary>
    /// Gets or sets the requested num of documents.
    /// </summary>
    [JsonProperty("size")]
    public int Size { get; set; }

    /// <summary>
    /// Gets or sets the query sorting value.
    /// </summary>
    [JsonProperty("sort")]
    public List<SortClause> Sort { get; set; }

    /// <summary>
    /// Gets or sets the query aggregations value.
    /// </summary>
    [JsonProperty("aggs")]
    public AggregationDictionary Aggregations { get; set; }

    /// <summary>
    /// Gets or sets the doc value field.
    /// Which allows to return the doc value representation of a field for each hit.
    /// </summary>
    [JsonProperty("docvalue_fields")]
    public List<DocValueField> DocValueFields { get; set; }

    /// <summary>
    /// Gets or sets the query highlight value.
    /// </summary>
    [JsonProperty("highlight")]
    public Highlight Highlight { get; set; }

    /// <summary>
    /// Gets or sets the query highlight text value.
    /// </summary>
    public Dictionary<string, string> HighlightText { get; set; }

    /// <summary>
    /// Gets or sets the index name to query.
    /// </summary>
    public string IndexName { get; set; }

    /// <inheritdoc/>
    public void Accept(IVisitor visitor)
    {
        visitor.Visit(this);
    }
}
