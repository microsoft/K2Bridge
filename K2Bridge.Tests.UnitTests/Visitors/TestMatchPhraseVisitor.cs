// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2BridgeUnitTests.Visitors
{
    using K2Bridge.Models.Request.Queries;
    using K2Bridge.Visitors;
    using NUnit.Framework;
    using Tests;

    [TestFixture]
    public class TestMatchPhraseVisitor
    {
        [TestCase(ExpectedResult = "MyField == \"MyPhrase\"")]
        public string TestValidMatchPhraseVisit()
        {
            var matchPhraseClause = CreateMatchPhraseClause("MyField", "MyPhrase");

            return VisitQuery(matchPhraseClause);
        }

        [TestCase(ExpectedResult = "MyField == \"\"")]
        public string TestMatchPhraseWithoutPhraseVisit()
        {
            var matchPhraseClause = CreateMatchPhraseClause("MyField", null);

            return VisitQuery(matchPhraseClause);
        }

        [TestCase]
        public void TestInvalidMatchPhraseVisit()
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