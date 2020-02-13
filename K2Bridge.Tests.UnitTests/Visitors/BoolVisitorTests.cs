// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace UnitTests.K2Bridge.Visitors
{
    using System.Collections.Generic;
    using global::K2Bridge.Models.Request;
    using global::K2Bridge.Models.Request.Queries;
    using global::K2Bridge.Visitors;
    using NUnit.Framework;

    [TestFixture]
    public class BoolVisitorTests
    {
        [TestCase(ExpectedResult = "(* has \"ItemA\")")]
        public string BoolQueryVisit_WithMustLeaf_ReturnsValidResponse()
        {
            var boolQuery = new BoolQuery
            {
                Must = CreateSimpleLeafList("ItemA"),
            };

            return VisitQuery(boolQuery);
        }

        [TestCase(ExpectedResult = "not (* has \"ItemB\")")]
        public string BoolQueryVisit_WithMustNotLeaf_ReturnsValidResponse()
        {
            var boolQuery = new BoolQuery
            {
                MustNot = CreateSimpleLeafList("ItemB"),
            };

            return VisitQuery(boolQuery);
        }

        [TestCase(ExpectedResult = "(* has \"ItemC\")")]
        public string BoolQueryVisit_WithShouldLeaf_ReturnsValidResponse()
        {
            var boolQuery = new BoolQuery
            {
                Should = CreateSimpleLeafList("ItemC"),
            };

            return VisitQuery(boolQuery);
        }

        [TestCase(ExpectedResult = "not (* has \"ItemD\")")]
        public string BoolQueryVisit_WithShouldNotLeaf_ReturnsValidResponse()
        {
            var boolQuery = new BoolQuery
            {
                ShouldNot = CreateSimpleLeafList("ItemD"),
            };

            return VisitQuery(boolQuery);
        }

        [TestCase(ExpectedResult = "(* has \"ItemA1\") and " +
            "(* has \"ItemA2\") and " +
            "(* has \"ItemA3\") and " +
            "(* has \"ItemA4\")")]
        public string BoolQueryVisit_WithMultipleMustLeaf_ReturnsValidResponse()
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

        [TestCase(ExpectedResult = "(* has \"ItemA\") and " +
            "not (* has \"ItemB\") or " +
            "(* has \"ItemC\") or " +
            "not (* has \"ItemD\")")]
        public string BoolQueryVisit_WithMultipleTypeLeafs_ReturnsValidResponse()
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

        private static IEnumerable<IQuery> CreateSimpleLeafList(string singleValue)
        {
            var lst = new LinkedList<string>();
            lst.AddFirst(singleValue);
            return CreateSimpleLeafList(lst);
        }

        private static IEnumerable<IQuery> CreateSimpleLeafList(IEnumerable<string> values)
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

        private string VisitQuery(BoolQuery query)
        {
            var visitor = new ElasticSearchDSLVisitor(SchemaRetrieverMock.CreateMockSchemaRetriever());
            visitor.Visit(query);
            return query.KustoQL;
        }
    }
}