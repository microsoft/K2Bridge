using K2Bridge;
using K2Bridge.Models.Request.Queries;
using K2Bridge.Visitors;
using NUnit.Framework;

namespace VisitorsTests
{
    [TestFixture]
    public class TestMatchPhraseVisitor
    {
        private string VisitQuery(MatchPhraseQuery clause)
        {
            var visitor = new ElasticSearchDSLVisitor();
            visitor.Visit(clause);
            return clause.KQL;
        }

        private static MatchPhraseQuery CreateMatchPhraseClause(string fieldName, string phrase)
        {
            return new MatchPhraseQuery
            {
                FieldName = fieldName,
                Phrase = phrase
            };
        }

        [TestCase(ExpectedResult = "MyField == \"MyPhrase\"")]
        public string TestValidMatchPhraseVisit()
        {
            var matchPhraseQuery = CreateMatchPhraseClause("MyField", "MyPhrase");

            return VisitQuery(matchPhraseQuery);
        }

        [TestCase(ExpectedResult = "MyField == \"\"")]
        public string TestMatchPhraseWithoutPhraseVisit()
        {
            var matchPhraseQuery = CreateMatchPhraseClause("MyField", null);

            return VisitQuery(matchPhraseQuery);
        }

        [TestCase]
        public void TestInvalidMatchPhraseVisit()
        {
            var matchPhraseQuery = CreateMatchPhraseClause(null, "myPhrase");


            Assert.Throws(typeof(IllegalClauseException), () => VisitQuery(matchPhraseQuery));
        }
    }
}