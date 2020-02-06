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

            matchPhraseClause.KustoQL = $"{matchPhraseClause.FieldName} {KustoQLOperators.Equal} \"{matchPhraseClause.Phrase}\"";
        }
    }
}
