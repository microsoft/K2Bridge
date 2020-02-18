// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2Bridge.Visitors
{
    using System.Collections.Generic;
    using K2Bridge.Models.Request;
    using K2Bridge.Models.Request.Queries;

    /// <content>
    /// A visitor for the <see cref="BoolQuery"/> element.
    /// </content>
    internal partial class ElasticSearchDSLVisitor : IVisitor
    {
        /// <inheritdoc/>
        public void Visit(BoolQuery boolQuery)
        {
            Ensure.IsNotNull(boolQuery, nameof(boolQuery));

            AddListInternal(boolQuery.Must, KustoQLOperators.And, false /* positive */, boolQuery);
            AddListInternal(boolQuery.MustNot, KustoQLOperators.And, true /* negative */, boolQuery);
            AddListInternal(boolQuery.Should, KustoQLOperators.Or, false /* positive */, boolQuery);
            AddListInternal(boolQuery.ShouldNot, KustoQLOperators.Or, true /* negative */, boolQuery);
        }

        /// <summary>
        /// Joins the list of KustoQL commands and modifies the given BoolQuery.
        /// </summary>
        private static void QueryListToString(List<string> queryList, string joinString, BoolQuery boolQuery)
        {
            joinString = $" {joinString} ";

            if (queryList?.Count > 0)
            {
                // In case we already have something in KustoQL, we need to first
                // add a separator
                if (!string.IsNullOrEmpty(boolQuery.KustoQL))
                {
                    boolQuery.KustoQL += $"{joinString}";
                }

                // Now, join the given list separated with the given word
                boolQuery.KustoQL += $"{string.Join(joinString, queryList)}";
            }
        }

        /// <summary>
        /// Create a list of clauses of a specific type (must / must not / should / should not).
        /// </summary>
        private void AddListInternal(IEnumerable<IQuery> list, string delimiterKeyword, bool negativeCondition, BoolQuery boolQuery)
        {
            if (list == null)
            {
                return;
            }

            var kqlExpressions = new List<string>();

            foreach (dynamic leafQuery in list)
            {
                if (leafQuery == null)
                {
                    // probably deserialization problem
                    continue;
                }

                leafQuery.Accept(this);
                if (leafQuery.KustoQL != null)
                {
                    kqlExpressions.Add($"{(negativeCondition ? $"{KustoQLOperators.Not} " : string.Empty)}({leafQuery.KustoQL})");
                }
            }

            QueryListToString(kqlExpressions, delimiterKeyword, boolQuery);
        }
    }
}