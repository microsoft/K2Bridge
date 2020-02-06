// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2Bridge.Visitors.LuceneNet
{
    using System;
    using K2Bridge.Models.Request.Queries;
    using K2Bridge.Models.Request.Queries.LuceneNet;
    using Lucene.Net.Search;

    /// <summary>
    /// Defines a visit method for lucene wildcard query.
    /// </summary>
    internal partial class LuceneVisitor : ILuceneVisitor
    {
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