using System;
using K2Bridge;
using K2Bridge.Models.Request.Queries;
using K2Bridge.Visitors;
using NUnit.Framework;

namespace VisitorsTests
{
    [TestFixture]
    public class TestRangeVisitor
    {
        private static string VisitRangeQuery(RangeQuery clause)
        {
            var visitor = new ElasticSearchDSLVisitor();
            visitor.Visit(clause);
            return clause.KQL;
        }

        private static RangeQuery CreateRangeQueryClause(string fieldName,
            Decimal? gte, Decimal? gt, Decimal? lte, Decimal? lt, String? format)
        {
            return new RangeQuery
            {
                FieldName = fieldName,
                GTEValue = gte,
                GTValue = gt,
                LTValue = lt,
                LTEValue = lte,
                Format = format
            };
        }

        [TestCase(ExpectedResult = "myField >= 3 and myField < 5")]
        public string TestBasicRangeVisitor()
        {
            var rangeClause = CreateRangeQueryClause("myField", 3, null, null, 5, "other");

            return VisitRangeQuery(rangeClause);
        }

        [TestCase(ExpectedResult =
            "myField >= fromUnixTimeMilli(1212121121) and myField <= fromUnixTimeMilli(2121212121)")]
        public string TestEpochBasicRangeVisitor()
        {
            var rangeClause = CreateRangeQueryClause("myField", 1212121121, null, 2121212121, null, "epoch_millis");

            return VisitRangeQuery(rangeClause);
        }

        [TestCase]
        public void TestMissingFieldNameRangeVisitor()
        {
            var rangeClause = CreateRangeQueryClause(null, 3, null, null, 5, "other");

            Assert.Throws(typeof(IllegalClauseException), () => VisitRangeQuery(rangeClause));
        }

        [TestCase]
        public void TestMissingGTERangeVisitor()
        {

            var rangeClause = CreateRangeQueryClause("myField", null, null, null, 5, "other");

            Assert.Throws(typeof(IllegalClauseException), () => VisitRangeQuery(rangeClause));
        }

        [TestCase]
        public void TestMissingLTRangeVisitor()
        {
            var rangeClause = CreateRangeQueryClause("myField", 5, null, null, null, "other");

            Assert.Throws(typeof(IllegalClauseException), () => VisitRangeQuery(rangeClause));
        }

        [TestCase]
        public void TestEpochMissingGTERangeVisitor()
        {
            var rangeClause = CreateRangeQueryClause("myField", null, null, null, 5, "epoch_millis");

            Assert.Throws(typeof(IllegalClauseException), () => VisitRangeQuery(rangeClause));
        }

        [TestCase]
        public void TestEpochMissingLTERangeVisitor()
        {
            var rangeClause = CreateRangeQueryClause("myField", 5, null, null, null, "epoch_millis");

            Assert.Throws(typeof(IllegalClauseException), () => VisitRangeQuery(rangeClause));
        }
    }
}