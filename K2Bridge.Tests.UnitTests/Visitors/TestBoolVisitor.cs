// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace VisitorsTests
{
    using System.Collections.Generic;
    using K2Bridge.Models.Request;
    using K2Bridge.Models.Request.Queries;
    using K2Bridge.Visitors;
    using NUnit.Framework;

    [TestFixture]
    public class TestBoolVisitor
    {
        [TestCase(ExpectedResult = "((* contains \"ItemA\"))")]
        public string TestOneBoolQueryInMustVisit()
        {
            var boolQuery = new BoolQuery
            {
                Must = CreateSimpleLeafList("ItemA"),
            };

            return VisitQuery(boolQuery);
        }

        [TestCase(ExpectedResult = "not ((* contains \"ItemB\"))")]
        public string TestOneBoolQueryInMustNotVisit()
        {
            var boolQuery = new BoolQuery
            {
                MustNot = CreateSimpleLeafList("ItemB"),
            };

            return VisitQuery(boolQuery);
        }

        [TestCase(ExpectedResult = "((* contains \"ItemC\"))")]
        public string TestOneBoolQueryInShouldVisit()
        {
            var boolQuery = new BoolQuery
            {
                Should = CreateSimpleLeafList("ItemC"),
            };

            return VisitQuery(boolQuery);
        }

        [TestCase(ExpectedResult = "not ((* contains \"ItemD\"))")]
        public string TestOneBoolQueryInShouldNotVisit()
        {
            var boolQuery = new BoolQuery
            {
                ShouldNot = CreateSimpleLeafList("ItemD"),
            };

            return VisitQuery(boolQuery);
        }

        [TestCase(ExpectedResult = "((* contains \"ItemA1\")) and " +
            "((* contains \"ItemA2\")) and " +
            "((* contains \"ItemA3\")) and " +
            "((* contains \"ItemA4\"))")]
        public string TestMultipleBoolQueryInMustVisit()
        {
            var lst = new LinkedList<string>();
            lst.AddLast("ItemA1");
            lst.AddLast("ItemA2");
            lst.AddLast("ItemA3");
            lst.AddLast("ItemA4");

            var boolQuery = new BoolQuery
            {
                Must = CreateSimpleLeafList(lst),
            };

            return VisitQuery(boolQuery);
        }

        [TestCase(ExpectedResult = "((* contains \"ItemA\"))\n| " +
            "where not ((* contains \"ItemB\"))\n| " +
            "where ((* contains \"ItemC\"))\n| " +
            "where not ((* contains \"ItemD\"))")]
        public string TestSimpleBoolQueryVisit()
        {
            var boolQuery = new BoolQuery
            {
                Must = CreateSimpleLeafList("ItemA"),
                MustNot = CreateSimpleLeafList("ItemB"),
                Should = CreateSimpleLeafList("ItemC"),
                ShouldNot = CreateSimpleLeafList("ItemD"),
            };

            return VisitQuery(boolQuery);
        }

        private string VisitQuery(BoolQuery query)
        {
            var visitor = new ElasticSearchDSLVisitor();
            visitor.Visit(query);
            return query.KQL;
        }

        private IEnumerable<IQuery> CreateSimpleLeafList(string singleValue)
        {
            var lst = new LinkedList<string>();
            lst.AddFirst(singleValue);
            return CreateSimpleLeafList(lst);
        }

        private IEnumerable<IQuery> CreateSimpleLeafList(IEnumerable<string> values)
        {
            var lst = new LinkedList<ILeafClause>();

            foreach (var value in values)
            {
                var item = new QueryStringClause
                {
                    Phrase = value,
                };

                lst.AddLast(item);
            }

            return lst;
        }
    }
}