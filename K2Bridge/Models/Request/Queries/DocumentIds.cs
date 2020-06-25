// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2Bridge.Models.Request.Queries
{
    using K2Bridge.Visitors;
    using Newtonsoft.Json;

    /// <summary>
    /// sadsadsa.
    /// </summary>
    internal class DocumentIds : KustoQLBase, IVisitable
    {
        /// <summary>
        /// sadasd.
        /// </summary>
        [JsonProperty("type")]
        public string Type { get; set; }

        /// <summary>
        /// sdfsdf.
        /// </summary>
        [JsonProperty("values")]
        public long[] Id { get; set; }

        /// <inheritdoc/>
        public void Accept(IVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
}
