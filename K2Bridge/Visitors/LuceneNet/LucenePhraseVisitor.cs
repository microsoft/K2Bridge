// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

using System.Text;
using K2Bridge.Models.Request.Queries;
using K2Bridge.Models.Request.Queries.LuceneNet;
using Lucene.Net.Index;
using Lucene.Net.Search;

namespace K2Bridge.Visitors.LuceneNet;

/// <content>
/// Defines a visit method for lucene phrase query.
/// </content>
internal partial class LuceneVisitor : ILuceneVisitor
{
    /// <inheritdoc/>
    public void Visit(LucenePhraseQuery phraseQueryWrapper)
    {
        VerifyValid(phraseQueryWrapper);

        var terms = ((PhraseQuery)phraseQueryWrapper.LuceneQuery).GetTerms();
        var phrase = TermsToString(terms);
        var clause = new QueryStringClause
        {
            ParsedFieldName = terms[0].Field,
            Phrase = phrase,
            ParsedType = QueryStringClause.Subtype.Phrase,
        };
        phraseQueryWrapper.ESQuery = clause;
    }

    private string TermsToString(Term[] terms)
    {
        var sb = new StringBuilder();
        foreach (var term in terms)
        {
            sb.Append($"{term.Text} ");
        }

        return sb.ToString().TrimEnd(' ');
    }
}
