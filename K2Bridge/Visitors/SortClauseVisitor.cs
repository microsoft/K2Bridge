// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2Bridge.Visitors
{
    using System;
    using K2Bridge.Models.Request;

    internal partial class ElasticSearchDSLVisitor : IVisitor
    {
        public void Visit(SortClause sortClause)
        {
            if (sortClause == null)
            {
                throw new ArgumentException(
                    "Argument cannot be null",
                    nameof(sortClause));
            }

            if (string.IsNullOrEmpty(sortClause.FieldName))
            {
                throw new IllegalClauseException(
                    "SortClause must have a valid FieldName property");
            }

            if (sortClause.FieldName.StartsWith('_'))
            {
                // fields that start with "_" are internal to elastic and we want to disregard them
                sortClause.KQL = string.Empty;
            }
            else
            {
                if (string.IsNullOrEmpty(sortClause.Order))
                {
                    throw new IllegalClauseException(
                        "SortClause must have a valid Order property");
                }

                sortClause.KQL = $"{sortClause.FieldName} {sortClause.Order}";
            }
        }
    }
}
