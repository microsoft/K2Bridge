// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2Bridge.Models.Request.Queries
{
    using K2Bridge.Models.Request;
    using K2Bridge.Visitors;
    using Newtonsoft.Json;

    /// <summary>
    /// sss.
    /// </summary>
    internal class SingleDocQuery : KustoQLBase, IVisitable
    {
        /// <summary>
        /// Gets or sets the id with which to perform the query.
        /// </summary>
        [JsonProperty("ids")]
        public DocumentIds DocumentId { get; set; }

        /// <inheritdoc/>
        public void Accept(IVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
}
