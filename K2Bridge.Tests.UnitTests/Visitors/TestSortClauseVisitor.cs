using System;
using K2Bridge.Models.Request;
using K2Bridge.Visitors;
using NUnit.Framework;

namespace VisitorsTests
{
    [TestFixture]
    public class TestSortClauseVisitor
    {
        [TestCase(ExpectedResult = "")]
        public string IgnoresClausesWithUnderscore()
        {
            var sortClause = new SortClause() { FieldName = "_wibble" };

            var visitor = new ElasticSearchDSLVisitor();
            visitor.Visit(sortClause);

            return sortClause.KQL;
        }

        [TestCase(ExpectedResult = "wibble asc")]
        public string GeneratesClauseQuery()
        {
            var sortClause = new SortClause() { FieldName = "wibble", Order = "asc" };

            var visitor = new ElasticSearchDSLVisitor();
            visitor.Visit(sortClause);

            return sortClause.KQL;
        }
    }
}
