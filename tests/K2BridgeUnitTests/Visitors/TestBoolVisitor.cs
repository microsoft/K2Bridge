using System.Collections.Generic;
using K2Bridge.Models.Request;
using K2Bridge.Models.Request.Queries;
using K2Bridge.Visitors;
using NUnit.Framework;

namespace VisitorsTests
{
    [TestFixture]
    public class TestBoolVisitor
    {
        private IEnumerable<IQueryClause> CreateSimpleLeafList(string singleValue)
        {
            var lst = new LinkedList<string>();
            lst.AddFirst(singleValue);
            return CreateSimpleLeafList(lst);
        }

        private IEnumerable<IQueryClause> CreateSimpleLeafList(IEnumerable<string> values)
        {
            var lst = new LinkedList<ILeafQueryClause>();

            foreach (var value in values)
            {
                var item = new QueryStringQuery
                {
                    Phrase = value
                };

                lst.AddLast(item);
            }

            return lst;
        }

        [TestCase(ExpectedResult = "((* contains \"ItemA\"))")]
        public string TestOneBoolClauseInMustVisit()
        {
            var boolClause = new BoolClause
            {
                Must = CreateSimpleLeafList("ItemA"),
            };

            var visitor = new ElasticSearchDSLVisitor();
            visitor.Visit(boolClause);
            return boolClause.KQL;
        }

        [TestCase(ExpectedResult = "not ((* contains \"ItemB\"))")]
        public string TestOneBoolClauseInMustNotVisit()
        {
            var boolClause = new BoolClause
            {
                MustNot = CreateSimpleLeafList("ItemB"),
            };

            var visitor = new ElasticSearchDSLVisitor();
            visitor.Visit(boolClause);
            return boolClause.KQL;
        }

        [TestCase(ExpectedResult = "((* contains \"ItemC\"))")]
        public string TestOneBoolClauseInShouldVisit()
        {
            var boolClause = new BoolClause
            {
                Should = CreateSimpleLeafList("ItemC"),
            };

            var visitor = new ElasticSearchDSLVisitor();
            visitor.Visit(boolClause);
            return boolClause.KQL;
        }

        [TestCase(ExpectedResult = "not ((* contains \"ItemD\"))")]
        public string TestOneBoolClauseInShouldNotVisit()
        {
            var boolClause = new BoolClause
            {
                ShouldNot = CreateSimpleLeafList("ItemD"),
            };

            var visitor = new ElasticSearchDSLVisitor();
            visitor.Visit(boolClause);
            return boolClause.KQL;
        }

        [TestCase(ExpectedResult = "((* contains \"ItemA1\")) and " +
            "((* contains \"ItemA2\")) and " +
            "((* contains \"ItemA3\")) and " +
            "((* contains \"ItemA4\"))")]
        public string TestMultipleBoolClauseInMustVisit()
        {
            var lst = new LinkedList<string>();
            lst.AddLast("ItemA1");
            lst.AddLast("ItemA2");
            lst.AddLast("ItemA3");
            lst.AddLast("ItemA4");

            var boolClause = new BoolClause
            {
                Must = CreateSimpleLeafList(lst)
            };

            var visitor = new ElasticSearchDSLVisitor();
            visitor.Visit(boolClause);
            return boolClause.KQL;
        }

        [TestCase(ExpectedResult = "((* contains \"ItemA\"))\n| " +
            "where not ((* contains \"ItemB\"))\n| " +
            "where ((* contains \"ItemC\"))\n| " +
            "where not ((* contains \"ItemD\"))")]
        public string TestSimpleBoolClauseVisit()
        {
            var boolClause = new BoolClause
            {
                Must = CreateSimpleLeafList("ItemA"),
                MustNot = CreateSimpleLeafList("ItemB"),
                Should = CreateSimpleLeafList("ItemC"),
                ShouldNot = CreateSimpleLeafList("ItemD")
            };

            var visitor = new ElasticSearchDSLVisitor();
            visitor.Visit(boolClause);
            return boolClause.KQL;
        }
    }
}