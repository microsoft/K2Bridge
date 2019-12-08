// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace K2Bridge.Visitors
{
    using K2Bridge.Models.Request.Queries;

    internal partial class ElasticSearchDSLVisitor : IVisitor
    {
        public void Visit(ExistsQuery existsQuery)
        {
            existsQuery.KQL = $"{KQLOperators.IsNotNull}({existsQuery.FieldName})";
        }
    }
}
