// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2Bridge.Visitors
{
    using System.Collections.Generic;
    using System.Text.RegularExpressions;
    using K2Bridge.Models.Request;
    using K2Bridge.Models.Request.Queries;

    /// <content>
    /// A visitor for the <see cref="BoolQuery"/> element.
    /// </content>
    internal partial class ElasticSearchDSLVisitor : IVisitor
    {
        private static readonly Regex OperatorsRegex = new Regex($"^[^ ]+ ({KustoQLOperators.Has}|{KustoQLOperators.Contains}|{KustoQLOperators.HasPrefix})|({KustoQLOperators.Equal})");

        /// <inheritdoc/>
        public void Visit(BoolQuery boolQuery)
        {
            Ensure.IsNotNull(boolQuery, nameof(boolQuery));

            AddListInternal(boolQuery.Filter, KustoQLOperators.And, false /* positive */, boolQuery);
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
        private void AddListInternal(IEnumerable<IQuery> list, string delimiterKeyword, bool negateCondition, BoolQuery boolQuery)
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
                    // Probably deserialization problem
                    continue;
                }

                leafQuery.Accept(this);
                if (leafQuery.KustoQL != null)
                {
                    string expression;

                    if (negateCondition)
                    {
                        var matcher = OperatorsRegex.Match(leafQuery.KustoQL);

                        if (matcher.Success)
                        {
                            expression = matcher.Groups[1].Success
                                ? $"({leafQuery.KustoQL.Insert(matcher.Groups[1].Index, "!")})"
                                : $"({leafQuery.KustoQL.Remove(matcher.Groups[2].Index, 1).Insert(matcher.Groups[2].Index, "!")})";
                        }
                        else
                        {
                            expression = $"{KustoQLOperators.Not} ({leafQuery.KustoQL})";
                        }
                    }
                    else
                    {
                        expression = $"({leafQuery.KustoQL})";
                    }

                    kqlExpressions.Add(expression);
                }
            }

            QueryListToString(kqlExpressions, delimiterKeyword, boolQuery);
        }
    }
}