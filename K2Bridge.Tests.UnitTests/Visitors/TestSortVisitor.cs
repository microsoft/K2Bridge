using K2Bridge;
using K2Bridge.Models.Request;
using K2Bridge.Visitors;
using NUnit.Framework;

namespace VisitorsTests
{
    [TestFixture]
    public class TestSortVisitor
    {
        private string VisitSortQuery(SortClause clause)
        {
            var visitor = new ElasticSearchDSLVisitor();
            visitor.Visit(clause);
            return clause.KQL;
        }

        [TestCase(ExpectedResult = "myFieldName ASC")]
        public string TestBasicSortVisitor()
        {
            var sortClause = new SortClause
            {
                FieldName = "myFieldName",
                Order = "ASC",
            };

            return VisitSortQuery(sortClause);
        }

        [TestCase(ExpectedResult = "")]
        public string TestInnerElasticSortVisitor()
        {
            var sortClause = new SortClause
            {
                FieldName = "_myInternalField",
            };

            return VisitSortQuery(sortClause);
        }

        [TestCase]
        public void TestMissingFieldSortVisitor()
        {
            var sortClause = new SortClause
            {
                Order = "Desc",
            };

            Assert.Throws(typeof(IllegalClauseException), () => VisitSortQuery(sortClause));
        }

        [TestCase]
        public void TestMissingOrderSortVisitor()
        {
            var sortClause = new SortClause
            {
                FieldName = "myField",
            };

            Assert.Throws(typeof(IllegalClauseException), () => VisitSortQuery(sortClause));
        }
    }
}