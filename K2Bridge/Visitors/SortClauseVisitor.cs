// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2Bridge.Visitors
{
    using K2Bridge.Models.Request;

    /// <content>
    /// A visitor for the <see cref="SortClause"/> element.
    /// </content>
    internal partial class ElasticSearchDSLVisitor : IVisitor
    {
        public void Visit(SortClause sortClause)
        {
            Ensure.IsNotNull(sortClause, nameof(sortClause));
            Ensure.IsNotNull(sortClause, nameof(sortClause));
            EnsureClause.StringIsNotNullOrEmpty(sortClause.FieldName, nameof(sortClause.FieldName));

            if (sortClause.FieldName.StartsWith('_'))
            {
                // fields that start with "_" are internal to elastic and we want to disregard them
                sortClause.KustoQL = string.Empty;
            }
            else
            {
                EnsureClause.StringIsNotNullOrEmpty(sortClause.Order, nameof(sortClause.Order));

                sortClause.KustoQL = $"{sortClause.FieldName} {sortClause.Order}";
            }
        }
    }
}
