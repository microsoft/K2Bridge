// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2Bridge.Models.Request.Queries
{
    using K2Bridge.Models.Request;
    using K2Bridge.Visitors;
    using Newtonsoft.Json;

    /// <summary>
    /// Full query to visit.
    /// </summary>
    internal class Query : KustoQLBase, IVisitable
    {
        /// <summary>
        /// Gets or sets bool query.
        /// </summary>
        [JsonProperty("bool")]
        public BoolQuery Bool { get; set; }

        /// <summary>
        /// Gets or sets the id with which to perform a ViewSingleDocument query.
        /// </summary>
        [JsonProperty("ids")]
        public DocumentIds Ids { get; set; }

        /// <inheritdoc/>
        public void Accept(IVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
}
