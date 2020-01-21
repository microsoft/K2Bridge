// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2BridgeUnitTests.Visitors
{
    using K2Bridge.Models.Request.Queries;
    using K2Bridge.Visitors;
    using NUnit.Framework;

    [TestFixture]
    public class TestQueryVisitor
    {
        [TestCase(ExpectedResult = "* == \"myPharse\"")]
        public string TestBasicQueryVisitor()
        {
            var queryClause = CreateQueryStringClause("myPharse", true);

            return VisitQuery(queryClause);
        }

        /*
        [TestCase(ExpectedResult = "TBD")]
        public string TestWildCardVisitor()
        {
            var queryClause = CreateQueryStringClause("myPharse", false);

            return VisitQuery(queryClause);
        }
        */

        [TestCase(ExpectedResult =
            "(* == \"myPharse\") and (* == \"herPhrase\")")]
        public string TestMultipleAndPharsesQueryVisitor()
        {
            var queryClause = CreateQueryStringClause("myPharse AND herPhrase", true);

            return VisitQuery(queryClause);
        }

        [TestCase(ExpectedResult =
            "(* == \"myPharse\") or (* == \"herPhrase\")")]
        public string TestMultipleOrPharsesQueryVisitor()
        {
            var queryClause = CreateQueryStringClause("myPharse OR herPhrase", true);

            return VisitQuery(queryClause);
        }

        [TestCase(ExpectedResult =
            "not (* == \"myPharse\") and not (* == \"herPhrase\")")]
        public string TestMultipleNotPharsesQueryVisitor()
        {
            var queryClause = CreateQueryStringClause("NOT myPharse AND NOT herPhrase", true);

            return VisitQuery(queryClause);
        }

        private static string VisitQuery(QueryStringClause queryStringClause)
        {
            var visitor = new ElasticSearchDSLVisitor();
            visitor.Visit(queryStringClause);
            return queryStringClause.KQL;
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