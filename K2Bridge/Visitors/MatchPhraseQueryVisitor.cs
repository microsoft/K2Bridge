// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2Bridge.Visitors
{
    using K2Bridge.Models.Request.Queries;

    internal partial class ElasticSearchDSLVisitor : IVisitor
    {
        public void Visit(MatchPhraseClause matchPhraseClause)
        {
            // Must have a field name
            if (string.IsNullOrEmpty(matchPhraseClause.FieldName))
            {
                throw new IllegalClauseException("FieldName must have a valid value");
            }

            matchPhraseClause.KQL = $"{matchPhraseClause.FieldName} == \"{matchPhraseClause.Phrase}\"";
        }
    }
}
