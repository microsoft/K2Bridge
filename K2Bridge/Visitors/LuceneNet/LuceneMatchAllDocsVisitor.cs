// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2Bridge.Visitors.LuceneNet
{
    using K2Bridge.Models.Request.Queries;
    using K2Bridge.Models.Request.Queries.LuceneNet;

    /// <content>
    /// Defines a visit method for lucene range query.
    /// </content>
    internal partial class LuceneVisitor : ILuceneVisitor
    {
        /// <inheritdoc/>
        public void Visit(LuceneMatchAllDocsQuery luceneMatchAllDocsQueryWrapper)
        {
            VerifyValid(luceneMatchAllDocsQueryWrapper);

            var wildcardClause = new QueryStringClause
            {
                ParsedType = QueryStringClause.Subtype.MatchAll,
            };

            luceneMatchAllDocsQueryWrapper.ESQuery = wildcardClause;
        }
    }
}
