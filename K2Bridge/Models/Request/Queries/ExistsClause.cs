// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2Bridge.Models.Request.Queries
{
    using K2Bridge.JsonConverters;
    using K2Bridge.Models.Request;
    using K2Bridge.Visitors;
    using Newtonsoft.Json;

    /// <summary>
    /// Clause to retrieve documents that contain a value in a specified field.
    /// </summary>
    [JsonConverter(typeof(ExistsClauseConverter))]
    public class ExistsClause : KustoQLBase, ILeafClause, IVisitable
    {
        /// <summary>
        /// Gets or sets the exists FieldName.
        /// </summary>
        public string FieldName { get; internal set; }

        /// <inheritdoc/>
        public void Accept(IVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
}
