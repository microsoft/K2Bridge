// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2Bridge.Visitors.LuceneNet
{
    using K2Bridge.Models.Request.Queries.LuceneNet;

    /// <summary>
    /// ILuceneVisitor defines all the different visit methods overloads to handle all supported lucene query types.
    /// </summary>
    internal interface ILuceneVisitor
    {
        void Visit(LuceneTermQuery termQueryWrapper);

        void Visit(LuceneBoolQuery boolQueryWrapper);

        void Visit(LucenePhraseQuery phraseQueryWrapper);

        void Visit(LucenePrefixQuery prefixQueryWrapper);

        void Visit(LuceneWildcardQuery wildcardQueryWrapper);

        void Visit(LuceneRangeQuery rangeQueryWrapper);
    }
}
