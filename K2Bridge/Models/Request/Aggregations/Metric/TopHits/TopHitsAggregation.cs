// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2Bridge.Models.Request.Aggregations
{
    using System.Linq;
    using System.Collections.Generic;
    using K2Bridge.Visitors;
    using Newtonsoft.Json;

    /// <summary>
    /// A top_hits metric aggregator keeps track of the most relevant document being aggregated.
    /// </summary>
    internal class TopHitsAggregation : MetricAggregation, IPartitionable
    {
        [JsonProperty("docvalue_fields")]
        public List<DocValueField> DocValueFields { get; set; }

        [JsonProperty("size")]
        public int? Size { get; set; }

        [JsonProperty("sort")]
        public List<SortClause> Sort { get; set; }

        [JsonIgnore]
        public override string Field => DocValueFields?.First()?.Field;

        [JsonIgnore]
        public string PartitionKey { get; set; }

        /// <inheritdoc/>
        public override void Accept(IVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
}