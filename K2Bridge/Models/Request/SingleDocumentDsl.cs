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
    /// sss.
    /// </summary>
    internal class SingleDocumentDsl : KustoQLBase, IVisitable
    {
        /// <summary>
        /// Gets or sets the query object.
        /// </summary>
        [JsonProperty("query")]
        public SingleDocQuery SingleDocQuery { get; set; }

        /// <summary>
        /// Gets or sets the doc value field.
        /// Which allows to return the doc value representation of a field for each hit.
        /// </summary>
        [JsonProperty("docvalue_fields")]
        public List<DocValueField> DocValueFields { get; set; }

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
}
