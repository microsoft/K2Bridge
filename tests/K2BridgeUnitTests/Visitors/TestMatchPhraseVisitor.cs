using System.Collections.Generic;
using K2Bridge.Models.Request;
using K2Bridge.Models.Request.Queries;
using K2Bridge.Visitors;
using NUnit.Framework;

namespace VisitorsTests
{
    [TestFixture]
    public class TestMatchPhraseVisitor
    {
        [TestCase(ExpectedResult = "MyField == \"MyPhrase\"")]
        public string TestValidMatchPhraseVisit()
        {
            var matchPhraseQuery = new MatchPhraseQuery
            {
                FieldName = "MyField",
                Phrase = "MyPhrase"
            };

            var visitor = new ElasticSearchDSLVisitor();
            visitor.Visit(matchPhraseQuery);
            return matchPhraseQuery.KQL;
        }

        [TestCase(ExpectedResult = "MyField == \"\"")]
        public string TestMatchPhraseWithoutPhraseVisit()
        {
            var matchPhraseQuery = new MatchPhraseQuery
            {
                FieldName = "MyField",
            };

            var visitor = new ElasticSearchDSLVisitor();
            visitor.Visit(matchPhraseQuery);
            return matchPhraseQuery.KQL;
        }

        [TestCase(ExpectedResult = "")]
        public string TestInvalidMatchPhraseVisit()
        {
            var matchPhraseQuery = new MatchPhraseQuery
            {
                Phrase = "phrase",
            };

            var visitor = new ElasticSearchDSLVisitor();
            visitor.Visit(matchPhraseQuery);
            return matchPhraseQuery.KQL;
        }
    }
}