// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2Bridge.Models.Request.Queries
{
    using K2Bridge.Visitors;
    using Newtonsoft.Json;

    /// <summary>
    /// A ViewSingleDocument query for a single doc id (represented by the _id column)
    /// </summary>
    internal class DocumentIds : KustoQLBase, IVisitable
    {
        /// <summary>
        /// Gets or sets the type of the required data.
        /// </summary>
        [JsonProperty("type")]
        public string Type { get; set; }

        /// <summary>
        /// Gets or sets the values for the required document (meaning: value of _id).
        /// </summary>
        [JsonProperty("values")]
        public string[] Id { get; set; }

        /// <inheritdoc/>
        public void Accept(IVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
}
