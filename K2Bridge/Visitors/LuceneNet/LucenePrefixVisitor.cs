// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2Bridge.Visitors.LuceneNet
{
    using K2Bridge.Models.Request.Queries;
    using K2Bridge.Models.Request.Queries.LuceneNet;
    using Lucene.Net.Search;

    /// <summary>
    /// Defines a visit method for lucene prefix query.
    /// </summary>
    internal partial class LuceneVisitor : ILuceneVisitor
    {
        public void Visit(LucenePrefixQuery prefixQueryWrapper)
        {
            VerifyValid(prefixQueryWrapper);

            var prefix = ((PrefixQuery)prefixQueryWrapper.LuceneQuery).Prefix;
            var prefixClause = new MatchPhraseClause
            {
                FieldName = prefix.Field,
                Phrase = prefix.Text,
                ClauseSubType = MatchPhraseClause.Subtype.Prefix,
            };
            prefixQueryWrapper.ESQuery = prefixClause;
        }
    }
}