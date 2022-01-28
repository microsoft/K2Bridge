// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace UnitTests.K2Bridge.Visitors
{
    using global::K2Bridge.Models.Request;
    using global::K2Bridge.Visitors;
    using NUnit.Framework;

    [TestFixture]
    public class SortVisitorTests
    {
        [TestCase(
            ExpectedResult = "['myFieldName'] ASC",
            TestName = "Visit_WithBasicInput_ReturnsExpectedResult")]
        public string TestBasicSortVisitor()
        {
            var sortClause = new SortClause
            {
                FieldName = "myFieldName",
                Order = "ASC",
            };

            return VisitSortQuery(sortClause);
        }

        [TestCase(
            ExpectedResult = "",
            TestName = "Visit_WithInnterElasticInput_Ignores")]
        public string TestInnerElasticSortVisitor()
        {
            var sortClause = new SortClause
            {
                FieldName = "_myInternalField",
            };

            return VisitSortQuery(sortClause);
        }

        [Test]
        public void Visit_WithMissingFields_ThrowsException()
        {
            var sortClause = new SortClause
            {
                Order = "Desc",
            };

            Assert.Throws(typeof(IllegalClauseException), () => VisitSortQuery(sortClause));
        }

        [Test]
        public void Visit_WithMissingOrder_ThrowsException()
        {
            var sortClause = new SortClause
            {
                FieldName = "myField",
            };

            Assert.Throws(typeof(IllegalClauseException), () => VisitSortQuery(sortClause));
        }

        private string VisitSortQuery(SortClause clause)
        {
            var visitor = new ElasticSearchDSLVisitor(SchemaRetrieverMock.CreateMockSchemaRetriever());
            visitor.Visit(clause);
            return clause.KustoQL;
        }
    }
}
