// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2Bridge.Visitors
{
    using System;
    using System.Text.RegularExpressions;
    using K2Bridge.Models.Request.Queries;
    using K2Bridge.Utils;

    /// <content>
    /// A visitor for the <see cref="MatchPhraseClause"/> element.
    /// </content>
    internal partial class ElasticSearchDSLVisitor : IVisitor
    {
        // The following regexes look for the '?' or '*' chars which are
        // not followed by an escape character
        private static readonly Regex SingleCharPattern = new (@"(?<!\\)\?");
        private static readonly Regex MultiCharPattern = new (@"(?<!\\)\*");

        /// <inheritdoc/>
        public void Visit(MatchPhraseClause matchPhraseClause)
        {
            Ensure.IsNotNull(matchPhraseClause, nameof(matchPhraseClause));

            // Must have a field name
            EnsureClause.StringIsNotNullOrEmpty(matchPhraseClause.FieldName, nameof(matchPhraseClause.FieldName));

            if (matchPhraseClause.Phrase != null)
            {
                var parsedPhrase = matchPhraseClause.Phrase switch
                {
                    DateTime dt => $"{KustoQLOperators.ToDateTime}(\"{dt.ToUniversalTime():o}\")",
                    uint or int or short or ushort or long or ulong or float or double => matchPhraseClause.Phrase,
                    object o => $"\"{matchPhraseClause.Phrase.ToString().EscapeSlashes()}\"",
                };

                matchPhraseClause.KustoQL = $"{EncodeKustoField(matchPhraseClause.FieldName)} {KustoQLOperators.Equal} {parsedPhrase}";
                return;
            }

            matchPhraseClause.KustoQL = $"{EncodeKustoField(matchPhraseClause.FieldName)} {KustoQLOperators.Equal} \"\"";
        }
    }
}
