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
            this.AddListInternal(boolQuery.Must, KQLOperators.And, false /* positive */, boolQuery);
            this.AddListInternal(boolQuery.MustNot, KQLOperators.And, true /* negative */, boolQuery);
            this.AddListInternal(boolQuery.Should, KQLOperators.Or, false /* positive */, boolQuery);
            this.AddListInternal(boolQuery.ShouldNot, KQLOperators.Or, true /* negative */, boolQuery);
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
                if (negativeCondition)
                {
                    kqlExpressions.Add($"{KQLOperators.Not} ({leafQuery.KQL})");
                }
                else
                {
                    kqlExpressions.Add($"({leafQuery.KQL})");
                }
            }

            this.KQLListToString(kqlExpressions, delimiterKeyword, boolQuery);
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
                    boolQuery.KQL += KQLOperators.CommandSeparator;
                    boolQuery.KQL += KQLOperators.Where;
                    boolQuery.KQL += " ";
                }

                boolQuery.KQL += $"{string.Join(joinString, kqlList)}";
            }
        }
    }
}