// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2Bridge.Visitors
{
    using K2Bridge.Models.Request.Queries;

    /// <content>
    /// A visitor for the <see cref="DocumentIds"/> element.
    /// </content>
    internal partial class ElasticSearchDSLVisitor : IVisitor
    {
        private const string IdColumnName = "_id";

        /// <inheritdoc/>
        public void Visit(DocumentIds documentIds)
        {
            Ensure.IsNotNull(documentIds, nameof(documentIds));
            Ensure.IsNotNull(documentIds.Id, nameof(documentIds.Id));
            Ensure.ConditionIsMet(documentIds.Id.Length == 1, $"{nameof(documentIds.Id)} must have exactly one document id");

            documentIds.KustoQL = $"{IdColumnName} {KustoQLOperators.Equal} \"{documentIds.Id[0]}\"";
        }
    }
}
