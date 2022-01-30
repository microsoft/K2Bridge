// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2Bridge.Models.Request.Queries.LuceneNet
{
    using K2Bridge.Visitors.LuceneNet;

    /// <summary>
    /// Represents a lucene range query.
    /// </summary>
    internal class LuceneRangeQuery : LuceneQueryBase, ILuceneVisitable
    {
        /// <inheritdoc/>
        public void Accept(ILuceneVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
}
