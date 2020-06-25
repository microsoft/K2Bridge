// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2Bridge.Visitors
{
    using K2Bridge.Models.Request.Queries;

    /// <content>
    /// A visitor for the <see cref="Query"/> and the <see cref="SingleDocQuery"/> elements.
    /// This includes all the "search" parts of an incoming request.
    /// </content>
    internal partial class ElasticSearchDSLVisitor : IVisitor
    {
        /// <inheritdoc/>
        public void Visit(Query query)
        {
            Ensure.IsNotNull(query, nameof(query));
            EnsureClause.IsNotNull(query.Bool, nameof(query.Bool));

            query.Bool.Accept(this);
            query.KustoQL = !string.IsNullOrEmpty(query.Bool.KustoQL) ? $"{KustoQLOperators.Where} {query.Bool.KustoQL}" : string.Empty;
        }

        /// <inheritdoc/>
        public void Visit(SingleDocQuery singleDocQuery)
        {
            Ensure.IsNotNull(singleDocQuery, nameof(singleDocQuery));
            EnsureClause.IsNotNull(singleDocQuery.DocumentId, nameof(singleDocQuery.DocumentId));

            singleDocQuery.DocumentId.Accept(this);
            singleDocQuery.KustoQL = !string.IsNullOrEmpty(singleDocQuery.DocumentId.KustoQL) ? $"{KustoQLOperators.Where} {singleDocQuery.DocumentId.KustoQL}" : string.Empty;
        }
    }
}
