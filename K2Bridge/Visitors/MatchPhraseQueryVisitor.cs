// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace K2Bridge.Visitors
{
    using K2Bridge.Models.Request.Queries;

    internal partial class ElasticSearchDSLVisitor : IVisitor
    {
        public void Visit(MatchPhrase matchPhrase)
        {
            // Must have a field name
            if (string.IsNullOrEmpty(matchPhrase.FieldName))
            {
                throw new IllegalClauseException("FieldName must have a valid value");
            }

            matchPhrase.KQL = $"{matchPhrase.FieldName} == \"{matchPhrase.Phrase}\"";
        }
    }
}
