// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2Bridge.Visitors
{
    using System.Text.RegularExpressions;
    using K2Bridge.Models.Request.Queries;

    internal partial class ElasticSearchDSLVisitor : IVisitor
    {
        // The following regexes look for the '?' or '*' chars which are
        // not followed by an escape character
        private static readonly Regex SingleCharPattern = new Regex(@"(?<!\\)\?");
        private static readonly Regex MultiCharPattern = new Regex(@"(?<!\\)\*");

        public void Visit(MatchPhraseClause matchPhraseClause)
        {
            Ensure.IsNotNull(matchPhraseClause, nameof(matchPhraseClause));

            // Must have a field name
            EnsureClause.StringIsNotNullOrEmpty(matchPhraseClause.FieldName, nameof(matchPhraseClause.FieldName));

            // Depends on the exact request there are 3 possible options for match phrase:
            // wildcard, prefix and simple equality
            switch (matchPhraseClause.ClauseSubType)
            {
                case MatchPhraseClause.Subtype.Wildcard:
                    // Now, each occurrence is replaced with [.\S] or [.\S]*
                    // This group is looking for any char except space, this is in order
                    // to be consistent with the way ES works
                    var phrase = SingleCharPattern.Replace(matchPhraseClause.Phrase, @"[.\\S]");
                    phrase = MultiCharPattern.Replace(phrase, @"[.\\S]*");

                    matchPhraseClause.KustoQL = $"{matchPhraseClause.FieldName} {KustoQLOperators.MatchRegex} \"{phrase}\"";
                    break;
                case MatchPhraseClause.Subtype.Prefix:
                    matchPhraseClause.KustoQL = $"{matchPhraseClause.FieldName} {KustoQLOperators.HasPrefixCS} \"{matchPhraseClause.Phrase}\"";
                    break;
                default:
                    // Simple subtype
                    matchPhraseClause.KustoQL = $"{matchPhraseClause.FieldName} {KustoQLOperators.Equal} \"{matchPhraseClause.Phrase}\"";
                    break;
            }
        }
    }
}
