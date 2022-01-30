// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

using K2Bridge.Visitors.LuceneNet;

namespace K2Bridge.Models.Request.Queries.LuceneNet;

/// <summary>
/// Represents a lucene prefix query.
/// </summary>
internal class LucenePrefixQuery : LuceneQueryBase, ILuceneVisitable
{
    /// <inheritdoc/>
    public void Accept(ILuceneVisitor visitor)
    {
        visitor.Visit(this);
    }
}
