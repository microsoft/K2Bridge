// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace UnitTests.K2Bridge.Visitors.LuceneNet
{
    using System;
    using global::K2Bridge.Models.Request.Queries;
    using global::K2Bridge.Models.Request.Queries.LuceneNet;
    using global::K2Bridge.Tests.UnitTests.Visitors;
    using global::K2Bridge.Visitors;
    using global::K2Bridge.Visitors.LuceneNet;
    using NUnit.Framework;

    [TestFixture]
    public class LuceneTermVisitorTests
    {
        [TestCase]
        public void Visit_WithNullTermQuery_ThrowsException()
        {
            var visitor = new LuceneVisitor();
            Assert.That(
                () => visitor.Visit((LuceneTermQuery)null),
                Throws.TypeOf<ArgumentNullException>());
        }

        [TestCase]
        public void Visit_WithInvalidTermQuery_ThrowsException()
        {
            var termQuery = new LuceneTermQuery
            {
                LuceneQuery = null,
            };
            var visitor = new LuceneVisitor();
            Assert.That(
                () => visitor.Visit(termQuery),
                Throws.TypeOf<IllegalClauseException>());
        }

        [TestCase(ExpectedResult = "* has \"Kfar-Saba\"")]
        public string Visit_WithValidWildcardTermQuery_ReturnsValidResponse()
        {
            var termQuery = new LuceneTermQuery
            {
                LuceneQuery =
                new Lucene.Net.Search.TermQuery(
                    new Lucene.Net.Index.Term("*", "Kfar-Saba")),
            };

            var luceneVisitor = new LuceneVisitor();
            luceneVisitor.Visit(termQuery);

            var es = termQuery.ESQuery;
            Assert.NotNull(es);

            var visitor = VisitorTestsUtils.CreateAndVisitRootVisitor();
            visitor.Visit((QueryStringClause)es);

            return ((QueryStringClause)es).KustoQL;
        }
    }
}
