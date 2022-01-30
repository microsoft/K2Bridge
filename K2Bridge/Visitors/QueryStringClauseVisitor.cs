// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2Bridge.Visitors;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using K2Bridge.Models.Request.Queries;
using K2Bridge.Models.Request.Queries.LuceneNet;
using K2Bridge.Utils;
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
        new() { " ", "AND", "OR", "NOT", "\"", ":", "(", ")", "[", "]", "{", "}", "*", "&&", "+", "-", "|", "?", "\\", "^", "~" };

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
        var parsedFieldName = queryStringClause.ParsedFieldName == "*" ? queryStringClause.ParsedFieldName : EncodeKustoField(queryStringClause.ParsedFieldName);
        switch (queryStringClause.ParsedType)
        {
            case QueryStringClause.Subtype.Term:
                var isNumeric = GetIsFieldNumeric(queryStringClause.ParsedFieldName).Result;
                if (isNumeric)
                {
                    // Check to see whether this Phrase contains just a numeric or >=, <=, > or > examples
                    if (decimal.TryParse(queryStringClause.Phrase, out var _))
                    {
                        queryStringClause.KustoQL = $"{parsedFieldName} {KustoQLOperators.Equal} {queryStringClause.Phrase}";
                    }
                    else
                    {
                        queryStringClause.KustoQL = $"{parsedFieldName} {queryStringClause.Phrase}";
                    }
                }
                else
                {
                    queryStringClause.KustoQL = $"{parsedFieldName} {KustoQLOperators.Has} \"{queryStringClause.Phrase.EscapeSlashesAndQuotes()}\"";
                }

                break;

            case QueryStringClause.Subtype.Phrase:
                queryStringClause.KustoQL = $"{parsedFieldName} {KustoQLOperators.Contains} \"{queryStringClause.Phrase.EscapeSlashesAndQuotes()}\"";
                break;

            case QueryStringClause.Subtype.Wildcard:
                // Now, each occurrence is replaced with [.\S] or [.\S]*
                // This group is looking for any char except space, this is in order
                // to be consistent with the way ES works
                // for example consider the following queries:
                // TelA* => TelA[.\S]*
                var phrase = SingleCharPattern.Replace(queryStringClause.Phrase.EscapeSlashesAndQuotes(), @"(.)");
                phrase = MultiCharPattern.Replace(phrase, @"(.)*");

                queryStringClause.KustoQL = $"{parsedFieldName} {KustoQLOperators.MatchRegex} \"{phrase}\"";
                break;
            case QueryStringClause.Subtype.Prefix:
                queryStringClause.KustoQL = $"{parsedFieldName} {KustoQLOperators.HasPrefix} \"{queryStringClause.Phrase.EscapeSlashesAndQuotes()}\"";
                break;
            case QueryStringClause.Subtype.MatchAll:
                queryStringClause.KustoQL = "true";
                break;
            default:
                // should not happen
                break;
        }
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
    private static bool IsSimplePhrase(string phrase)
    {
        return string.IsNullOrEmpty(phrase) || !SpecialStrings.Any(s => phrase.Contains(s, StringComparison.OrdinalIgnoreCase));
    }

    private async Task<bool> GetIsFieldNumeric(string fieldName)
    {
        Ensure.IsNotNullOrEmpty(fieldName, nameof(fieldName));
        var fieldType = await ClauseFieldTypeProcessor.GetType(schemaRetriever, fieldName);
        return fieldType == ClauseFieldType.Numeric;
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

        // We don't need to escape the phrase here, we want to keep it as-is so we can use escape sequences in lucene (such as \")
        var query = queryParser.Parse(queryStringClause.Phrase);

        // We make our own 'visitable' Lucence.Net query model
        var luceneQuery = VisitableLuceneQueryFactory.Make(query);

        // Visit
        var luceneVisitor = new LuceneVisitor();
        luceneQuery.Accept(luceneVisitor);
        dynamic esQuery = luceneQuery.ESQuery;
        esQuery.Accept(this);

        return esQuery.KustoQL;
    }
}
