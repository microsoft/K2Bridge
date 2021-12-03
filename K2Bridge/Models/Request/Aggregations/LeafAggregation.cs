// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2Bridge.Models.Request.Aggregations
{
    using K2Bridge.JsonConverters;
    using K2Bridge.Models.Request;
    using K2Bridge.Visitors;
    using Newtonsoft.Json;

    /// <summary>
    /// Leaf Aggregation to visit.
    /// </summary>
    [JsonConverter(typeof(LeafAggregationConverter))]
    internal abstract class LeafAggregation : KustoQLBase, IVisitable
    {
        /// <summary>
        /// Gets or Sets field alias.
        /// </summary>
        public string FieldAlias { get; set; }

        /// <summary>
        /// Gets or sets the translation of the query that needs to be inserted before 'summarize'.
        /// </summary>
        public string KustoQLPreSummarize { get; set; }

        /// <inheritdoc/>
        public abstract void Accept(IVisitor visitor);
    }
}
