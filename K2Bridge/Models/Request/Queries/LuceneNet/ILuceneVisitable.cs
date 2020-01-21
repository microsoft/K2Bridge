// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2Bridge.Models.Request.Queries.LuceneNet
{
    using K2Bridge.Visitors.LuceneNet;
    using Lucene.Net.Search;

    internal interface ILuceneVisitable
    {
        Query LuceneQuery { get; set; }

        IQuery ESQuery { get; set; }

        void Accept(ILuceneVisitor visitor);
    }
}
