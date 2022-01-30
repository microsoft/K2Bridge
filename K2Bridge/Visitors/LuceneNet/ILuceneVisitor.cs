// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2Bridge.Visitors.LuceneNet;

using K2Bridge.Models.Request.Queries.LuceneNet;

/// <summary>
/// ILuceneVisitor defines all the different visit methods overloads to handle all supported lucene query types.
/// </summary>
internal interface ILuceneVisitor
{
    /// <summary>
    /// Accepts a given lucene visitable object and builds a Kusto query.
    /// </summary>
    /// <param name="termQueryWrapper">The LuceneTermQuery object to visit.</param>
    void Visit(LuceneTermQuery termQueryWrapper);

    /// <summary>
    /// Accepts a given lucene visitable object and builds a Kusto query.
    /// </summary>
    /// <param name="boolQueryWrapper">The LuceneBoolQuery object to visit.</param>
    void Visit(LuceneBoolQuery boolQueryWrapper);

    /// <summary>
    /// Accepts a given lucene visitable object and builds a Kusto query.
    /// </summary>
    /// <param name="phraseQueryWrapper">The LucenePhraseQuery object to visit.</param>
    void Visit(LucenePhraseQuery phraseQueryWrapper);

    /// <summary>
    /// Accepts a given lucene visitable object and builds a Kusto query.
    /// </summary>
    /// <param name="prefixQueryWrapper">The LucenePrefixQuery object to visit.</param>
    void Visit(LucenePrefixQuery prefixQueryWrapper);

    /// <summary>
    /// Accepts a given lucene visitable object and builds a Kusto query.
    /// </summary>
    /// <param name="wildcardQueryWrapper">The LuceneWildcardQuery object to visit.</param>
    void Visit(LuceneWildcardQuery wildcardQueryWrapper);

    /// <summary>
    /// Accepts a given lucene visitable object and builds a Kusto query.
    /// </summary>
    /// <param name="rangeQueryWrapper">The LuceneRangeQuery object to visit.</param>
    void Visit(LuceneRangeQuery rangeQueryWrapper);

    /// <summary>
    /// Accepts a given lucene visitable object and builds a Kusto query.
    /// </summary>
    /// <param name="luceneMatchAllDocsQueryWrapper">The LuceneMatchAllDocsQuery object to visit.</param>
    void Visit(LuceneMatchAllDocsQuery luceneMatchAllDocsQueryWrapper);
}
