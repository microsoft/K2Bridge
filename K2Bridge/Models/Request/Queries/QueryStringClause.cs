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
            /// A pharse to be searched exactly (equality).
            /// </summary>
            Term = 0,

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

        // Lucene type properties
        // These will be initially null when the object
        // is created from a json payload
        public Subtype? ParsedType { get; set; }

        public string ParsedFieldName { get; set; }

        // original properties of QueryStringClause
        public string Phrase { get; set; }

        public bool Wildcard { get; set; }

        public string Default { get; set; }

        public void Accept(IVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
}