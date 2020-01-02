// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2Bridge.Models.Request.Queries
{
    using K2Bridge.Models.Request;
    using K2Bridge.Visitors;
    using Newtonsoft.Json;

    [JsonConverter(typeof(QueryStringClauseConverter))]
    internal class QueryStringClause : KQLBase, ILeafClause, IVisitable
    {
        public string Phrase { get; set; }

        public bool Wildcard { get; set; }

        public string Default { get; set; }

        public void Accept(IVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
}