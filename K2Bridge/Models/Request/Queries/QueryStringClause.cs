// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2Bridge.Models.Request.Queries
{
    using K2Bridge.JsonConverters;
    using K2Bridge.Models.Request;
    using K2Bridge.Visitors;
    using Newtonsoft.Json;

    [JsonConverter(typeof(QueryStringClauseConverter))]
    internal class QueryStringClause : KustoQLBase, ILeafClause, IVisitable
    {
        public enum Subtype
        {
            /// <summary>
            /// A pharse which is a word (no tokens inside).
            /// </summary>
            Term = 0,

            /// <summary>
            /// A pharse which can be multiple terms (a sentance with spaces).
            /// </summary>
            Phrase,

            /// <summary>
            /// A phrase to be searched as a prefix of a bigger term.
            /// </summary>
            Prefix,

            /// <summary>
            /// A phrase containing wildcards ('?' or '*').
            /// </summary>
            Wildcard,
        }

        /// <summary>
        /// Gets or sets Lucene type properties.
        /// These will be initially null when the object is created from a json payload,
        /// and will be populated by the visitor.
        /// </summary>
        public Subtype? ParsedType { get; set; }

        public string ParsedFieldName { get; set; }

        // original properties of QueryStringClause
        public string Phrase { get; set; }

        public bool Wildcard { get; set; }

        public string Default { get; set; }

        /// <inheritdoc/>
        public void Accept(IVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
}