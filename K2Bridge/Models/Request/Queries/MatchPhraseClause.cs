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
    /// Match phrase is looking for exact match of a phrase and the field value.
    /// </summary>
    [JsonConverter(typeof(MatchPhraseClauseConverter))]
    public class MatchPhraseClause : KustoQLBase, IVisitable, ILeafClause
    {
        /// <summary>
        /// Gets or sets the field to match.
        /// </summary>
        public string FieldName { get; internal set; }

        /// <summary>
        /// Gets or sets the matching phrase.
        /// </summary>
        public object Phrase { get; internal set; }

        /// <inheritdoc/>
        public void Accept(IVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
}