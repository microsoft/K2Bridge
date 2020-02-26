// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2Bridge.Visitors
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using K2Bridge.Models.Request.Queries;
    using K2Bridge.Models.Request.Queries.LuceneNet;
    using K2Bridge.Visitors.LuceneNet;
    using Lucene.Net.Analysis;
    using Lucene.Net.QueryParsers;

    /// <content>
    /// A visitor for the <see cref="QueryStringClause"/> element.
    /// This includes the main logic able to translate expressions from Kibana's searchbox.
    /// </content>
    internal partial class ElasticSearchDSLVisitor : IVisitor
    {
        /// <summary>
        /// A list of strings that when found, means the search term is a Lucene term.
        /// </summary>
        private static readonly List<string> SpecialStrings =
            new List<string> { " ", "AND", "OR", "NOT", "\"", ":", "(", ")", "[", "]", "{", "}", "*", "&&", "+", "-", "|", "?", "\\", "^", "~" };

        /// <inheritdoc/>
        public void Visit(QueryStringClause queryStringClause)
        {
            Ensure.IsNotNull(queryStringClause, nameof(queryStringClause));

            if (queryStringClause.ParsedType == null)
            {
                queryStringClause.KustoQL = IsSimplePhrase(queryStringClause.Phrase)
                    ? $"* {KustoQLOperators.Has} \"{queryStringClause.Phrase}\""
                    : CreateKqlFromLucenePhrase(queryStringClause);
                return;
            }

            // Depends on the exact request there are 3 possible options for the phrase:
            // wildcard, prefix and simple equality
            switch (queryStringClause.ParsedType)
            {
                case QueryStringClause.Subtype.Term:
                    var isMatchPhraseFieldType = IsMatchPhraseFieldType(queryStringClause.ParsedFieldName).Result;
                    if (isMatchPhraseFieldType)
                    {
                        var matchPhraseClause = new MatchPhraseClause()
                        {
                            FieldName = queryStringClause.ParsedFieldName,
                            Phrase = queryStringClause.Phrase,
                        };

                        matchPhraseClause.Accept(this);
                        queryStringClause.KustoQL = matchPhraseClause.KustoQL;
                    }
                    else
                    {
                        queryStringClause.KustoQL = $"{queryStringClause.ParsedFieldName} {KustoQLOperators.Has} \"{queryStringClause.Phrase}\"";
                    }

                    break;

                case QueryStringClause.Subtype.Phrase:
                    queryStringClause.KustoQL = $"{queryStringClause.ParsedFieldName} {KustoQLOperators.Contains} \"{queryStringClause.Phrase}\"";
                    break;

                case QueryStringClause.Subtype.Wildcard:
                    // Now, each occurrence is replaced with [.\S] or [.\S]*
                    // This group is looking for any char except space, this is in order
                    // to be consistent with the way ES works
                    // for example consider the following queries:
                    // TelA* => TelA[.\S]*
                    var phrase = SingleCharPattern.Replace(queryStringClause.Phrase, @"(.)");
                    phrase = MultiCharPattern.Replace(phrase, @"(.)*");

                    queryStringClause.KustoQL = $"{queryStringClause.ParsedFieldName} {KustoQLOperators.MatchRegex} \"{phrase}\"";
                    break;
                case QueryStringClause.Subtype.Prefix:
                    queryStringClause.KustoQL = $"{queryStringClause.ParsedFieldName} {KustoQLOperators.HasPrefix} \"{queryStringClause.Phrase}\"";
                    break;
                default:
                    // should not happen
                    break;
            }
        }

        private async Task<bool> IsMatchPhraseFieldType(string fieldName)
        {
            // for tests
            if (schemaRetriever == null)
            {
                return false;
            }

            var dic = await schemaRetriever.RetrieveTableSchema();

            // if we failed to get this field type, treat as non numeric (string)
            if (dic.Contains(fieldName) == false)
            {
                return false;
            }

            var fieldType = dic[fieldName];

            return fieldType switch
            {
                "integer" => true,
                "long" => true,
                "float" => true,
                "double" => true,
                "boolean" => true,
                _ => false,
            };
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

            // escaping special charachters from the pharse before parsing.
            // we would call QueryParser.Escape() method, but it escapes all charachters and
            // in our case we only have to worry about backslash.
            // implementation is based on: https://github.com/apache/lucenenet/blob/0eaf76540b8de326d1aa9ca24f4b5d6425a9ae38/src/Lucene.Net.QueryParser/Classic/QueryParserBase.cs
            var escapedPhrase = queryStringClause.Phrase.Replace(@"\", @"\\\", StringComparison.OrdinalIgnoreCase);

            // we parse and get the Lucene.Net query model
            var query = queryParser.Parse(escapedPhrase);

            // We make our own 'visitable' Lucence.Net query model
            var luceneQuery = VisitableLuceneQueryFactory.Make(query);

            // Visit
            var luceneVisitor = new LuceneVisitor();
            luceneQuery.Accept(luceneVisitor);
            dynamic esQuery = luceneQuery.ESQuery;
            esQuery.Accept(this);

            return esQuery.KustoQL;
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

            return !SpecialStrings.Any(s => phrase.Contains(s, StringComparison.OrdinalIgnoreCase));
        }
    }
}