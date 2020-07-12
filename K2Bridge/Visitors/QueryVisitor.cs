// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2Bridge.Visitors
{
    using K2Bridge.Models.Request.Queries;

    /// <content>
    /// A visitor for the <see cref="Query"/> element.
    /// This includes all the "search" parts of an incoming request.
    /// </content>
    internal partial class ElasticSearchDSLVisitor : IVisitor
    {
        /// <inheritdoc/>
        public void Visit(Query query)
        {
            Ensure.IsNotNull(query, nameof(query));

            string kustoQL;

            if (query.Bool != null)
            {
                // Data query
                query.Bool.Accept(this);
                kustoQL = query.Bool.KustoQL;
            }
            else if (query.Ids != null)
            {
                // View Single Document query
                query.Ids.Accept(this);
                kustoQL = query.Ids.KustoQL;
            }
            else
            {
                throw new IllegalClauseException("Either Bool or Ids clauses must not be null");
            }

            query.KustoQL = !string.IsNullOrEmpty(kustoQL) ? $"{KustoQLOperators.Where} {kustoQL}" : string.Empty;
        }
    }
}
