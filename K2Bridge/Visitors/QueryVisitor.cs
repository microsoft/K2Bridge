// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2Bridge.Visitors
{
    using K2Bridge.Models.Request.Queries;

    internal partial class ElasticSearchDSLVisitor : IVisitor
    {
        public void Visit(Query query)
        {
            Ensure.IsNotNull(query, nameof(query));
            EnsureClause.IsNotNull(query.Bool, nameof(query.Bool));

            query.Bool.Accept(this);
            query.KustoQL = !string.IsNullOrEmpty(query.Bool.KustoQL) ? $"{KustoQLOperators.Where} {query.Bool.KustoQL}" : string.Empty;
        }
    }
}
