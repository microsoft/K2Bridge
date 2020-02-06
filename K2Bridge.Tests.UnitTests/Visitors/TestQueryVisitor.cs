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
    public class TestQueryVisitor
    {
        private static readonly object[] MultiWordTestCases = {
            new TestCaseData("somePhrase otherPhrase").Returns("(* has \"somePhrase\") or (* has \"otherPhrase\")").SetName("QueryStringVisit_TwoWordPhrases_ReturnsExpectedValues"),
            new TestCaseData("somePhrase otherPhrase someOtherPhrase").Returns("(* has \"somePhrase\") or (* has \"otherPhrase\") or (* has \"someOtherPhrase\")").SetName("QueryStringVisit_ThreeWordPhrases_ReturnsExpectedValues"),
            new TestCaseData("   somePhrase  ").Returns("* has \"somePhrase\"").SetName("QueryStringVisit_EmptySpacePhrases_ReturnsExpectedValues"),
        };

        [TestCase(ExpectedResult = "* has \"myPharse\"")]
        public string TestBasicQueryVisitor()
        {
            var queryClause = CreateQueryStringClause("myPharse", true);

            return VisitQuery(queryClause);
        }

        [TestCase(ExpectedResult =
            "(* has \"myPharse\") and (* has \"herPhrase\")")]
        public string TestMultipleAndPharsesQueryVisitor()
        {
            var queryClause = CreateQueryStringClause("myPharse AND herPhrase", true);

            return VisitQuery(queryClause);
        }

        [TestCase(ExpectedResult =
            "(* has \"myPharse\") or (* has \"herPhrase\")")]
        public string TestMultipleOrPharsesQueryVisitor()
        {
            var queryClause = CreateQueryStringClause("myPharse OR herPhrase", true);

            return VisitQuery(queryClause);
        }

        [TestCase(ExpectedResult =
            "not (* has \"myPharse\") and not (* has \"herPhrase\")")]
        public string TestMultipleNotPharsesQueryVisitor()
        {
            var queryClause = CreateQueryStringClause("NOT myPharse AND NOT herPhrase", true);

            return VisitQuery(queryClause);
        }

        [TestCase(ExpectedResult =
            "(* has \"Dogs\") and (* contains \"My cats\")")]
        public string TestQuotationQueryVisitor()
        {
            var queryClause = CreateQueryStringClause("Dogs AND \"My cats\"", true);

            return VisitQuery(queryClause);
        }

        [TestCase(ExpectedResult =
            "(* has \"Tokyo\") and (* contains \"Haneda International\") or ((* has \"A\") and (* contains \"b c\"))")]
        public string TestMoreQuotationQueryVisitor()
        {
            var queryClause = CreateQueryStringClause("Tokyo AND \"Haneda International\" OR (A AND \"b c\")", true);

            return VisitQuery(queryClause);
        }

        private static string VisitQuery(QueryStringClause queryStringClause)
        {
            var visitor = new ElasticSearchDSLVisitor(LazySchemaRetrieverMock.CreateMockSchemaRetriever());
            visitor.Visit(queryStringClause);
            return queryStringClause.KustoQL;
        }

        private static QueryStringClause CreateQueryStringClause(string phrase, bool wildcard)
        {
            return new QueryStringClause
            {
                Phrase = phrase,
                Wildcard = wildcard,
                Default = "*",
            };
        }
    }
}