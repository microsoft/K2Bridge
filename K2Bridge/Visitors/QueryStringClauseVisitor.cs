// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2Bridge.Visitors
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using K2Bridge.Models.Request.Queries;
    using K2Bridge.Models.Request.Queries.LuceneNet;
    using K2Bridge.Visitors.LuceneNet;
    using Lucene.Net.Analysis;
    using Lucene.Net.QueryParsers;

    internal partial class ElasticSearchDSLVisitor : IVisitor
    {
        /// <summary>
        /// A list of strings that when found, means the search term is a Lucene term.
        /// </summary>
        private static readonly List<string> SpecialStrings =
            new List<string> { "AND", "OR", "NOT", "\"", ":", "(", ")", "[", "]", "{", "}", "*", "&&", "+", "-", "|", "?", "\\", "^", "~" };

        /// <summary>
        /// Accept a query string clause, parse the phrase to a lucene query, and build a Kusto query based on the lucene query.
        /// </summary>
        /// <param name="queryStringClause">The query string clause.</param>
        public void Visit(QueryStringClause queryStringClause)
        {
            if (queryStringClause == null)
            {
                throw new ArgumentException(
                    "Argument cannot be null",
                    nameof(queryStringClause));
            }

            if (IsSimplePhrase(queryStringClause.Phrase))
            {
                // search the phrase as is in Kusto
                queryStringClause.KQL =
                    $"* == \"{queryStringClause.Phrase}\"";
                return;
            }

            queryStringClause.KQL = CreateKqlFromLucenePhrase(queryStringClause);
        }

        /// <summary>
        /// Given a clause which has a Lucene based phrase, creates a KQL query.
        /// </summary>
        /// <param name="queryStringClause">The given clausse.</param>
        /// <returns>A KQL query string.</returns>
        private string CreateKqlFromLucenePhrase(QueryStringClause queryStringClause)
        {
            // we need to parse the phrase
            using var analyzer = new WhitespaceAnalyzer();
            var queryParser =
                new QueryParser(
                    Lucene.Net.Util.Version.LUCENE_30,
                    queryStringClause.Default,
                    analyzer)
                {
                    AllowLeadingWildcard = queryStringClause.Wildcard,
                    LowercaseExpandedTerms = false,
                };

            // we parse and get the Lucene.Net query model
            var query = queryParser.Parse(queryStringClause.Phrase);

            // We make our own 'visitable' Lucence.Net query model
            var luceneQuery = VisitableLuceneQueryFactory.Make(query);

            // Visit
            var luceneVisitor = new LuceneVisitor();
            luceneQuery.Accept(luceneVisitor);
            dynamic esQuery = luceneQuery.ESQuery;
            esQuery.Accept(this);

            return esQuery.KQL;
        }

        /// <summary>
        /// Checks whether the given string is a 'simple' pharse.
        /// a simple pharse is a phrase that does not contains any
        /// special keywords such as 'AND', 'OR', ':' and more.
        /// These keywords indicates that its not a simple phrase, its
        /// a Lucene expression.
        /// </summary>
        /// <param name="phrase">The phrase to check.</param>
        /// <returns>true if the given phrase is 'simple'.</returns>
        private bool IsSimplePhrase(string phrase)
        {
            if (string.IsNullOrEmpty(phrase))
            {
                return true;
            }

            return !SpecialStrings.Any(s => phrase.Contains(s, StringComparison.CurrentCulture));
        }
    }
}