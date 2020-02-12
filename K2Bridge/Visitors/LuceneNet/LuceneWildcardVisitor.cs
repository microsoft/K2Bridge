// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2Bridge.Visitors.LuceneNet
{
    using K2Bridge.Models.Request.Queries;
    using K2Bridge.Models.Request.Queries.LuceneNet;
    using Lucene.Net.Search;

    /// <content>
    /// Defines a visit method for lucene wildcard query.
    /// </content>
    internal partial class LuceneVisitor : ILuceneVisitor
    {
        /// <inheritdoc/>
        public void Visit(LuceneWildcardQuery wildcardQueryWrapper)
        {
            VerifyValid(wildcardQueryWrapper);

            var term = ((WildcardQuery)wildcardQueryWrapper.LuceneQuery).Term;
            var wildcardClause = new QueryStringClause
            {
                ParsedFieldName = term.Field,
                Phrase = term.Text,
                ParsedType = QueryStringClause.Subtype.Wildcard,
            };
            wildcardQueryWrapper.ESQuery = wildcardClause;
        }
    }
}