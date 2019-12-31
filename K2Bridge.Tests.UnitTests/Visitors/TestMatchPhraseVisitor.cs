using K2Bridge;
using K2Bridge.Models.Request.Queries;
using K2Bridge.Visitors;
using NUnit.Framework;

namespace VisitorsTests
{
    [TestFixture]
    public class TestMatchPhraseVisitor
    {
        private string VisitQuery(MatchPhrase clause)
        {
            var visitor = new ElasticSearchDSLVisitor();
            visitor.Visit(clause);
            return clause.KQL;
        }

        private static MatchPhrase CreateMatchPhraseClause(string fieldName, string phrase)
        {
            return new MatchPhrase
            {
                FieldName = fieldName,
                Phrase = phrase
            };
        }

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
    }
}