// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2Bridge.Visitors
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Text.RegularExpressions;
    using K2Bridge.Models.Request;
    using K2Bridge.Models.Request.Queries;

    /// <content>
    /// A visitor for the <see cref="BoolQuery"/> element.
    /// </content>
    internal partial class ElasticSearchDSLVisitor : IVisitor
    {
        private static readonly Regex OperatorsRegex = new ($"^[^ ]+ ({KustoQLOperators.Has}|{KustoQLOperators.Contains}|{KustoQLOperators.HasPrefix})|({KustoQLOperators.Equal})");
        private static readonly Regex DynamicRegex = new ("^((\\.)|[^ !=])+");

        /// <inheritdoc/>
        public void Visit(BoolQuery boolQuery)
        {
            Ensure.IsNotNull(boolQuery, nameof(boolQuery));
            var kustoQuery = new StringBuilder(boolQuery.KustoQL);

            AddListInternal(boolQuery.Filter, KustoQLOperators.And, false /* positive */, kustoQuery);
            AddListInternal(boolQuery.Must, KustoQLOperators.And, false /* positive */, kustoQuery);
            AddListInternal(boolQuery.MustNot, KustoQLOperators.And, true /* negative */, kustoQuery);
            AddListInternal(boolQuery.Should, KustoQLOperators.Or, false /* positive */, kustoQuery);
            AddListInternal(boolQuery.ShouldNot, KustoQLOperators.Or, true /* negative */, kustoQuery);

            if (kustoQuery.Length > 0 && (boolQuery.KustoQL == null || kustoQuery.Length > boolQuery.KustoQL.Length))
            {
                boolQuery.KustoQL = kustoQuery.ToString();
            }
        }

        /// <summary>
        /// Joins the list of KustoQL commands and modifies the given StringBuilder.
        /// </summary>
        private static void QueryListToString(ICollection<string> queryList, string joinString, StringBuilder kustoQuery)
        {
            var paddedJoinString = $" {joinString} ";

            // In case we already have something in KustoQL, we need to first add a separator
            if (kustoQuery.Length > 0)
            {
                kustoQuery.Append($"{paddedJoinString}");
            }

            // Group2 will be matched if a period character '.' is found in field name, meaning the filter is on a dynamic column.
            // OrderBy will sort the list such that non-dynamic filters will be followed by dynamic ones, which improves Kusto query performance.
            var orderedList = queryList.OrderBy(exp => DynamicRegex.Match(exp).Groups[2].Success);

            kustoQuery.Append($"{string.Join(paddedJoinString, orderedList)}");
        }

        /// <summary>
        /// Create a list of clauses of a specific type (must / must not / should / should not).
        /// </summary>
        private void AddListInternal(IEnumerable<IQuery> list, string delimiterKeyword, bool negateCondition, StringBuilder kustoQuery)
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

            if (kqlExpressions.Count > 0)
            {
                QueryListToString(kqlExpressions, delimiterKeyword, kustoQuery);
            }
        }
    }
}