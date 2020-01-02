// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2Bridge.Models.Request.Queries
{
    using System.Collections.Generic;
    using K2Bridge.Models.Request;
    using K2Bridge.Visitors;

    /// <summary>
    /// Represents a boolean clause.
    /// </summary>
    internal class BoolQuery : KQLBase, IVisitable, IQuery
    {
        public IEnumerable<IQuery> Must { get; set; }

        public IEnumerable<IQuery> MustNot { get; set; }

        public IEnumerable<IQuery> Should { get; set; }

        public IEnumerable<IQuery> ShouldNot { get; set; }

        public void Accept(IVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
}
