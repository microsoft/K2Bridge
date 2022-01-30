// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2Bridge.Visitors
{
    using K2Bridge.Models.Request.Queries;

    /// <content>
    /// A visitor for the <see cref="ExistsClause"/> element.
    /// </content>
    internal partial class ElasticSearchDSLVisitor : IVisitor
    {
        /// <inheritdoc/>
        public void Visit(ExistsClause existsClause)
        {
            Ensure.IsNotNull(existsClause, nameof(existsClause));
            EnsureClause.StringIsNotNullOrEmpty(existsClause.FieldName, nameof(existsClause.FieldName));

            existsClause.KustoQL = $"{KustoQLOperators.IsNotNull}({EncodeKustoField(existsClause.FieldName)})";
        }
    }
}
