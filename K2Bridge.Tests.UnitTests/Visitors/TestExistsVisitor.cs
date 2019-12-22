using System.Collections.Generic;
using K2Bridge.Models.Request;
using K2Bridge.Models.Request.Queries;
using K2Bridge.Visitors;
using NUnit.Framework;

namespace VisitorsTests
{
    [TestFixture]
    public class TestExistsVisitor
    {
        [TestCase(ExpectedResult = "isnotnull(MyField)")]
        public string TestValidExistsVisit()
        {
            var existsQuery = new ExistsQuery
            {
                FieldName = "MyField"
            };

            var visitor = new ElasticSearchDSLVisitor();
            visitor.Visit(existsQuery);
            return existsQuery.KQL;
        }
    }
}