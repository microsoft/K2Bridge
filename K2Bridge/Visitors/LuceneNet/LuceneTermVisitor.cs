// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2Bridge.Visitors.LuceneNet
{
    using K2Bridge.Models.Request.Queries;
    using K2Bridge.Models.Request.Queries.LuceneNet;
    using Lucene.Net.Search;

    /// <content>
    /// Defines a visit method for lucene term query.
    /// </content>
    internal partial class LuceneVisitor : ILuceneVisitor
    {
        /// <inheritdoc/>
        public void Visit(LuceneTermQuery termQueryWrapper)
        {
            VerifyValid(termQueryWrapper);

            var term = ((TermQuery)termQueryWrapper.LuceneQuery).Term;
            var clause = new QueryStringClause
            {
                ParsedFieldName = term.Field,
                Phrase = term.Text,
                ParsedType = QueryStringClause.Subtype.Term,
            };
            termQueryWrapper.ESQuery = clause;
        }
    }
}
