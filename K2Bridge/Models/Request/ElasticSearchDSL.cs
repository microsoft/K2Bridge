// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2Bridge.Models.Request
{
    using System.Collections.Generic;
    using K2Bridge.Models.Request.Aggregations;
    using K2Bridge.Models.Request.Queries;
    using K2Bridge.Visitors;
    using Newtonsoft.Json;

    /// <summary>
    /// ElasticSearchDSL (Elastic Search Domain Specific Language) represents
    /// the different properties of the elasticsearch query as deserialized from
    /// the json object sent from Kibana. This object will be sent for transformation.
    /// </summary>
    internal class ElasticSearchDSL : KustoQLBase, IVisitable
    {
        [JsonProperty("query")]
        public Query Query { get; set; }

        [JsonProperty("size")]
        public int Size { get; set; }

        [JsonProperty("sort")]
        public List<SortClause> Sort { get; set; }

        [JsonProperty("aggs")]
        public Dictionary<string, Aggregation> Aggregations { get; set; }

        [JsonProperty("highlight")]
        public Highlight Highlight { get; set; }

        public Dictionary<string, string> HighlightText { get; set; }

        public string IndexName { get; set; }

        public void Accept(IVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
}
