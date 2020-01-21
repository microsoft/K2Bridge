// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2Bridge.Models.Request.Queries.LuceneNet
{
    using K2Bridge.Visitors.LuceneNet;

    /// <summary>
    /// Represents a lucene phrase query.
    /// </summary>
    internal class LucenePhraseQuery : LuceneQueryBase, ILuceneVisitable
    {
        public void Accept(ILuceneVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
}
