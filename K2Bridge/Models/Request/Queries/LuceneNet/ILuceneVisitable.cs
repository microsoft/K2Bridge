// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

using K2Bridge.Visitors.LuceneNet;

namespace K2Bridge.Models.Request.Queries.LuceneNet;
using Query = Lucene.Net.Search.Query;

/// <summary>
/// An interface to note a visitable Lucene class.
/// </summary>
internal interface ILuceneVisitable
{
    Query LuceneQuery { get; set; }

    IQuery ESQuery { get; set; }

    /// <summary>
    /// This method performs the action (i.e. the visit).
    /// </summary>
    /// <param name="visitor">The <see cref="ILuceneVisitor"/> implemented class performing the action.</param>
    void Accept(ILuceneVisitor visitor);
}
