// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2Bridge.Visitors
{
    using System;
    using K2Bridge.Models.Request.Queries;

    internal partial class ElasticSearchDSLVisitor : IVisitor
    {
        public void Visit(Query query)
        {
            if (query == null)
            {
                throw new ArgumentException(
                    "Argument cannot be null",
                    nameof(query));
            }

            if (query.Bool == null)
            {
                throw new IllegalClauseException("invalid inner BoolQuery");
            }

            query.Bool.Accept(this);
            query.KQL = !string.IsNullOrEmpty(query.Bool.KQL) ? $"{KQLOperators.Where} {query.Bool.KQL}" : string.Empty;
      }
    }
}
