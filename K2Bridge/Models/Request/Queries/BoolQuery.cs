// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2Bridge.Models.Request.Queries;

using System.Collections.Generic;
using K2Bridge.Models.Request;
using K2Bridge.Visitors;

/// <summary>
/// A query that matches documents by boolean combinations of other queries.
/// </summary>
internal class BoolQuery : KustoQLBase, IVisitable, IQuery
{
    /// <summary>
    /// Gets or sets Must value in bool query.
    /// </summary>
    public IEnumerable<IQuery> Must { get; set; }

    /// <summary>
    /// Gets or sets MustNot value in bool query.
    /// </summary>
    public IEnumerable<IQuery> MustNot { get; set; }

    /// <summary>
    /// Gets or sets Should value in bool query.
    /// </summary>
    public IEnumerable<IQuery> Should { get; set; }

    /// <summary>
    /// Gets or sets ShouldNot value in bool query.
    /// </summary>
    public IEnumerable<IQuery> ShouldNot { get; set; }

    /// <summary>
    /// Gets or sets the expressions for filtering documents.
    /// This applies before other search expressions in the query class (like Must).
    /// </summary>
    public IEnumerable<IQuery> Filter { get; set; }

    /// <inheritdoc/>
    public void Accept(IVisitor visitor)
    {
        visitor.Visit(this);
    }
}
