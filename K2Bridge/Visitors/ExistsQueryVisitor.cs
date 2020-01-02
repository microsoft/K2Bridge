// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2Bridge.Visitors
{
    using K2Bridge.Models.Request.Queries;

    internal partial class ElasticSearchDSLVisitor : IVisitor
    {
        public void Visit(ExistsClause existsClause)
        {
            existsClause.KQL = $"{KQLOperators.IsNotNull}({existsClause.FieldName})";
        }
    }
}
