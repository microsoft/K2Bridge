using System;
using K2Bridge;
using K2Bridge.Models.Request.Queries;
using K2Bridge.Visitors;
using NUnit.Framework;
using Range = K2Bridge.Models.Request.Queries.Range;

namespace VisitorsTests
{
    [TestFixture]
    public class TestRangeVisitor
    {
        private static string VisitRangeClause(Range clause)
        {
            var visitor = new ElasticSearchDSLVisitor();
            visitor.Visit(clause);
            return clause.KQL;
        }

        private static Range CreateRangeClause(string fieldName,
            Decimal? gte, Decimal? gt, Decimal? lte, Decimal? lt, String? format)
        {
            return new Range
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
            var rangeClause = CreateRangeClause("myField", 3, null, null, 5, "other");

            return VisitRangeClause(rangeClause);
        }

        [TestCase(ExpectedResult =
            "myField >= fromUnixTimeMilli(1212121121) and myField <= fromUnixTimeMilli(2121212121)")]
        public string TestEpochBasicRangeVisitor()
        {
            var rangeClause = CreateRangeClause("myField", 1212121121, null, 2121212121, null, "epoch_millis");

            return VisitRangeClause(rangeClause);
        }

        [TestCase]
        public void TestMissingFieldNameRangeVisitor()
        {
            var rangeClause = CreateRangeClause(null, 3, null, null, 5, "other");

            Assert.Throws(typeof(IllegalClauseException), () => VisitRangeClause(rangeClause));
        }

        [TestCase]
        public void TestMissingGTERangeVisitor()
        {

            var rangeClause = CreateRangeClause("myField", null, null, null, 5, "other");

            Assert.Throws(typeof(IllegalClauseException), () => VisitRangeClause(rangeClause));
        }

        [TestCase]
        public void TestMissingLTRangeVisitor()
        {
            var rangeClause = CreateRangeClause("myField", 5, null, null, null, "other");

            Assert.Throws(typeof(IllegalClauseException), () => VisitRangeClause(rangeClause));
        }

        [TestCase]
        public void TestEpochMissingGTERangeVisitor()
        {
            var rangeClause = CreateRangeClause("myField", null, null, null, 5, "epoch_millis");

            Assert.Throws(typeof(IllegalClauseException), () => VisitRangeClause(rangeClause));
        }

        [TestCase]
        public void TestEpochMissingLTERangeVisitor()
        {
            var rangeClause = CreateRangeClause("myField", 5, null, null, null, "epoch_millis");

            Assert.Throws(typeof(IllegalClauseException), () => VisitRangeClause(rangeClause));
        }
    }
}