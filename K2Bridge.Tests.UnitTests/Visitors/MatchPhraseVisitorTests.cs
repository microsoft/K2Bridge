// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace UnitTests.K2Bridge.Visitors
{
    using global::K2Bridge.Models.Request.Queries;
    using global::K2Bridge.Visitors;
    using NUnit.Framework;

    [TestFixture]
    public class MatchPhraseVisitorTests
    {
        [TestCase(ExpectedResult = "MyField == \"MyPhrase\"")]
        public string MatchPhraseVisit_WithValidClause_ReturnsEquals()
        {
            var matchPhraseClause = CreateMatchPhraseClause("MyField", "MyPhrase");

            return VisitQuery(matchPhraseClause);
        }

        [TestCase(ExpectedResult = "MyField == \"\"")]
        public string MatchPhraseVisit_WithoutClause_ReturnsEqualsEmpty()
        {
            var matchPhraseClause = CreateMatchPhraseClause("MyField", null);

            return VisitQuery(matchPhraseClause);
        }

        [TestCase]
        public void MatchPhraseVisit_WithInvalidClause_ReturnsEqualsEmpty()
        {
            var matchPhraseClause = CreateMatchPhraseClause(null, "myPhrase");

            Assert.Throws(typeof(IllegalClauseException), () => VisitQuery(matchPhraseClause));
        }

        private static MatchPhraseClause CreateMatchPhraseClause(string fieldName, string phrase)
        {
            return new MatchPhraseClause
            {
                FieldName = fieldName,
                Phrase = phrase,
            };
        }

        private string VisitQuery(MatchPhraseClause clause)
        {
            var visitor = new ElasticSearchDSLVisitor(SchemaRetrieverMock.CreateMockSchemaRetriever());
            visitor.Visit(clause);
            return clause.KustoQL;
        }
    }
}