// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace K2Bridge.Models.Request.Queries
{
    using System.Collections.Generic;
    using K2Bridge.Models.Request;
    using K2Bridge.Visitors;

    internal class BoolClause : KQLBase, IVisitable, IQueryClause
    {
        public IEnumerable<IQueryClause> Must { get; set; }

        public IEnumerable<IQueryClause> MustNot { get; set; }

        public IEnumerable<IQueryClause> Should { get; set; }

        public IEnumerable<IQueryClause> ShouldNot { get; set; }

        public void Accept(IVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
}
