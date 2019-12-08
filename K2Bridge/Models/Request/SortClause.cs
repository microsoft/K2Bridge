// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace K2Bridge.Models.Request
{
    using K2Bridge.Visitors;
    using Newtonsoft.Json;

    [JsonConverter(typeof(SortClauseConverter))]
    internal class SortClause : KQLBase, IVisitable
    {
        public string FieldName { get; set; }

        public string Order { get; set; }

        public void Accept(IVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
}
