// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace UnitTests.K2Bridge.Visitors
{
    using global::K2Bridge.Models.Request.Queries;
    using global::K2Bridge.Visitors;
    using NUnit.Framework;

    [TestFixture]
    public class QueryStringClauseVisitorTests
    {
        [TestCase(ExpectedResult = "* has \"myPharse\"")]
        public string Visit_WithSimplePhrase_ReturnsHasResponse()
        {
            var queryClause = CreateQueryStringClause("myPharse", true);

            return VisitQuery(queryClause);
        }

        [TestCase(ExpectedResult =
            "(* has \"myPharse\") and (* has \"herPhrase\")")]
        public string Visit_WithMultiplePhrase_ReturnsHasAndResponse()
        {
            var queryClause = CreateQueryStringClause("myPharse AND herPhrase", true);

            return VisitQuery(queryClause);
        }

        [TestCase(ExpectedResult =
            "(* has \"myPharse\") or (* has \"herPhrase\")")]
        public string Visit_WithMultipleOrPhrase_ReturnsHasOrResponse()
        {
            var queryClause = CreateQueryStringClause("myPharse OR herPhrase", true);

            return VisitQuery(queryClause);
        }

        [TestCase(ExpectedResult =
            "not (* has \"myPharse\") and not (* has \"herPhrase\")")]
        public string Visit_WithMultipleNotPhrase_ReturnsHasAndNotResponse()
        {
            var queryClause = CreateQueryStringClause("NOT myPharse AND NOT herPhrase", true);

            return VisitQuery(queryClause);
        }

        [TestCase(ExpectedResult =
            "(* has \"Dogs\") and (* contains \"My cats\")")]
        public string Visit_WithQuotationPhrase_ReturnsContainsResponse()
        {
            var queryClause = CreateQueryStringClause("Dogs AND \"My cats\"", true);

            return VisitQuery(queryClause);
        }

        [TestCase(ExpectedResult =
            "(* has \"Tokyo\") and (* contains \"Haneda International\") or ((* has \"A\") and (* contains \"b c\"))")]
        public string Visit_WithMultipleQuotationPhrase_ReturnsAndContainsResponse()
        {
            var queryClause = CreateQueryStringClause("Tokyo AND \"Haneda International\" OR (A AND \"b c\")", true);

            return VisitQuery(queryClause);
        }

        [TestCase(ExpectedResult =
            "* has \"\\\"Get\"")]
        public string Visit_WithBreakQuotationPhrase_ReturnsAndContainsResponse()
        {
            var queryClause = CreateQueryStringClause("\\\"Get", false);

            return VisitQuery(queryClause);
        }

        [TestCase(ExpectedResult =
            "* has \"\\dev\\kusto\\K2Bridge\"")]
        public string Visit_WithBreakQuotePhrase_ReturnsAndContainsResponse()
        {
            var queryClause = CreateQueryStringClause(@"\dev\kusto\K2Bridge", false);

            return VisitQuery(queryClause);
        }

        [TestCase(ExpectedResult = null)]
        public string Visit_WithEmptyQuotePhrase_ReturnsAndContainsResponse()
        {
            var queryClause = CreateQueryStringClause("\"\"", false);

            return VisitQuery(queryClause);
        }

        private static string VisitQuery(QueryStringClause queryStringClause)
        {
            var visitor = new ElasticSearchDSLVisitor(SchemaRetrieverMock.CreateMockSchemaRetriever());
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