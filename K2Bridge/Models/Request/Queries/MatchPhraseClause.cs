// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2Bridge.Models.Request.Queries
{
    using K2Bridge.JsonConverters;
    using K2Bridge.Models.Request;
    using K2Bridge.Visitors;
    using Newtonsoft.Json;

    [JsonConverter(typeof(MatchPhraseClauseConverter))]
    internal class MatchPhraseClause : KQLBase, IVisitable, ILeafClause
    {
        public enum Subtype
        {
            /// <summary>
            /// A pharse to be searched exactly (equality).
            /// </summary>
            Simple = 0,

            /// <summary>
            /// A phrase to be searched as a prefix of a bigger term.
            /// </summary>
            Prefix,

            /// <summary>
            /// A phrase containing wildcards ('?' or '*').
            /// </summary>
            Wildcard,
        }

        public string FieldName { get; set; }

        public string Phrase { get; set; }

        public Subtype ClauseSubType { get; set; }

        public void Accept(IVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
}
