// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2Bridge.Models.Request.Queries.LuceneNet
{
    using System;
    using Lucene.Net.Search;

    /// <summary>
    /// Factory for making visitable lucene queries.
    /// </summary>
    internal static class VisitableLuceneQueryFactory
    {
        /// <summary>
        /// Create appropriate visitable lucene query object.
        /// </summary>
        /// <param name="query">
        /// A Lucene.Net query.
        /// </param>
        /// <returns>
        /// Our own visitable lucene query.
        /// </returns>
        public static ILuceneVisitable Make(Query query)
        {
            switch (query)
            {
                case BooleanQuery q:
                    return new LuceneBoolQuery
                    {
                        LuceneQuery = q,
                    };
                case TermQuery q:
                    return new LuceneTermQuery
                    {
                        LuceneQuery = q,
                    };
                case PhraseQuery q:
                    var lucenePhraseQuery = new LucenePhraseQuery
                    {
                        LuceneQuery = q,
                    };
                    return lucenePhraseQuery;
                case PrefixQuery q:
                    var lucenePrefixQuery = new LucenePrefixQuery
                    {
                        LuceneQuery = q,
                    };
                    return lucenePrefixQuery;
                case WildcardQuery q:
                    var luceneWildcardQuery = new LuceneWildcardQuery
                    {
                        LuceneQuery = q,
                    };
                    return luceneWildcardQuery;
                case TermRangeQuery q:
                    var luceneRangeQuery = new LuceneRangeQuery
                    {
                        LuceneQuery = q,
                    };
                    return luceneRangeQuery;
                case MatchAllDocsQuery q:
                    var luceneMatchAllDocsQuery = new LuceneMatchAllDocsQuery
                    {
                        LuceneQuery = q,
                    };
                    return luceneMatchAllDocsQuery;
                default:
                    throw new NotImplementedException();
            }
        }
    }
}
