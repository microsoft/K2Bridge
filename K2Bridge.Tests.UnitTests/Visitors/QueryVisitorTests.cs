// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace UnitTests.K2Bridge.Visitors
{
    using global::K2Bridge.Models.Request.Queries;
    using global::K2Bridge.Visitors;
    using NUnit.Framework;

    [TestFixture]
    public class QueryVisitorTests
    {
        [TestCase(ExpectedResult = "* has \"myPharse\"")]
        public string QueryVisit_WithSimplePhrase_ReturnsHasResponse()
        {
            var queryClause = CreateQueryStringClause("myPharse", true);

            return VisitQuery(queryClause);
        }

        [TestCase(ExpectedResult =
            "(* has \"myPharse\") and (* has \"herPhrase\")")]
        public string QueryVisit_WithMultiplePhrase_ReturnsHasAndResponse()
        {
            var queryClause = CreateQueryStringClause("myPharse AND herPhrase", true);

            return VisitQuery(queryClause);
        }

        [TestCase(ExpectedResult =
            "(* has \"myPharse\") or (* has \"herPhrase\")")]
        public string QueryVisit_WithMultipleOrPhrase_ReturnsHasOrResponse()
        {
            var queryClause = CreateQueryStringClause("myPharse OR herPhrase", true);

            return VisitQuery(queryClause);
        }

        [TestCase(ExpectedResult =
            "not (* has \"myPharse\") and not (* has \"herPhrase\")")]
        public string QueryVisit_WithMultipleNotPhrase_ReturnsHasAndNotResponse()
        {
            var queryClause = CreateQueryStringClause("NOT myPharse AND NOT herPhrase", true);

            return VisitQuery(queryClause);
        }

        [TestCase(ExpectedResult =
            "(* has \"Dogs\") and (* contains \"My cats\")")]
        public string QueryVisit_WithQuotationPhrase_ReturnsContainsResponse()
        {
            var queryClause = CreateQueryStringClause("Dogs AND \"My cats\"", true);

            return VisitQuery(queryClause);
        }

        [TestCase(ExpectedResult =
            "(* has \"Tokyo\") and (* contains \"Haneda International\") or ((* has \"A\") and (* contains \"b c\"))")]
        public string QueryVisit_WithMultipleQuotationPhrase_ReturnsAndContainsResponse()
        {
            var queryClause = CreateQueryStringClause("Tokyo AND \"Haneda International\" OR (A AND \"b c\")", true);

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