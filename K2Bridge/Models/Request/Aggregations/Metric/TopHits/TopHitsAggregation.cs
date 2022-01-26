// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2Bridge.Models.Request.Aggregations.Metric.TopHits
{
    using System.Linq;
    using System.Collections.Generic;
    using K2Bridge.Visitors;
    using Newtonsoft.Json;
    using K2Bridge.Models.Request.Aggregations.Metric;

    /// <summary>
    /// A top_hits metric aggregator keeps track of the most relevant document being aggregated.
    /// </summary>
    internal class TopHitsAggregation : PartitionableMetricAggregation
    {
        [JsonProperty("docvalue_fields")]
        public List<DocValueField> DocValueFields { get; set; }

        [JsonProperty("size")]
        public int? Size { get; set; }

        [JsonProperty("sort")]
        public List<SortClause> Sort { get; set; }

        [JsonIgnore]
        public override string Field => DocValueFields?.First()?.Field;

        /// <inheritdoc/>
        public override void Accept(IVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
}
