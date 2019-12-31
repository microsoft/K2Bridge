using K2Bridge.Models.Request.Queries;
using K2Bridge.Visitors;
using NUnit.Framework;

namespace VisitorsTests
{
    [TestFixture]
    public class TestQueryVisitor
    {
        private static string VisitQuery(QueryString clause)
        {
            var visitor = new ElasticSearchDSLVisitor();
            visitor.Visit(clause);
            return clause.KQL;
        }

        private static QueryString CreateQueryStringClause(string phrase, bool wildcard)
        {
            return new QueryString
            {
                Phrase = phrase,
                Wildcard = wildcard
            };
        }

        [TestCase(ExpectedResult = "(* contains \"myPharse\")")]
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
            "(* contains \"myPharse\") and (* contains \"herPhrase\")")]
        public string TestMultipleAndPharsesQueryVisitor()
        {
            var queryClause = CreateQueryStringClause("myPharse AND herPhrase", true);

            return VisitQuery(queryClause);
        }

        [TestCase(ExpectedResult =
            "(* contains \"myPharse\") or (* contains \"herPhrase\")")]
        public string TestMultipleOrPharsesQueryVisitor()
        {
            var queryClause = CreateQueryStringClause("myPharse OR herPhrase", true);

            return VisitQuery(queryClause);
        }

        [TestCase(ExpectedResult =
            "not (* contains \"myPharse\") and not (* contains \"herPhrase\")")]
        public string TestMultipleNotPharsesQueryVisitor()
        {
            var queryClause = CreateQueryStringClause("NOT myPharse AND NOT herPhrase", true);

            return VisitQuery(queryClause);
        }
    }
}