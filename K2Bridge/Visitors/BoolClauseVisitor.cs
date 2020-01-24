// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2Bridge.Visitors
{
    using System.Collections.Generic;
    using K2Bridge.Models.Request;
    using K2Bridge.Models.Request.Queries;

    internal partial class ElasticSearchDSLVisitor : IVisitor
    {
        public void Visit(BoolQuery boolQuery)
        {
            Ensure.IsNotNull(boolQuery, nameof(boolQuery));

            AddListInternal(boolQuery.Must, KQLOperators.And, false /* positive */, boolQuery);
            AddListInternal(boolQuery.MustNot, KQLOperators.And, true /* negative */, boolQuery);
            AddListInternal(boolQuery.Should, KQLOperators.Or, false /* positive */, boolQuery);
            AddListInternal(boolQuery.ShouldNot, KQLOperators.Or, true /* negative */, boolQuery);
        }

        /// <summary>
        /// Create a list of clauses of a specific type (must / must not / should / should not).
        /// </summary>
        private void AddListInternal(IEnumerable<IQuery> lst, string delimiterKeyword, bool negativeCondition, BoolQuery boolQuery)
        {
            if (lst == null)
            {
                return;
            }

            var kqlExpressions = new List<string>();

            foreach (dynamic leafQuery in lst)
            {
                if (leafQuery == null)
                {
                    // probably deserialization problem
                    continue;
                }

                leafQuery.Accept(this);
                kqlExpressions.Add($"{(negativeCondition ? $"{KQLOperators.Not} " : string.Empty)}({leafQuery.KQL})");
            }

            KQLListToString(kqlExpressions, delimiterKeyword, boolQuery);
        }

        /// <summary>
        /// Joins the list of kql commands and modifies the given BoolQuery.
        /// </summary>
        private void KQLListToString(List<string> kqlList, string joinString, BoolQuery boolQuery)
        {
            joinString = $" {joinString} ";

            if (kqlList.Count > 0)
            {
                if (!string.IsNullOrEmpty(boolQuery.KQL))
                {
                    boolQuery.KQL += $" {KQLOperators.And}\n "; // query is now complex - can not use where operator
                }

                boolQuery.KQL += $"{string.Join(joinString, kqlList)}";
            }
        }
    }
}