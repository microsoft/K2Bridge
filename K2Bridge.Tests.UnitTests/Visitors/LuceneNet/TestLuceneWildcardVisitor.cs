// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2BridgeUnitTests.Visitors.LuceneNet
{
    using System;
    using K2Bridge.Models.Request.Queries;
    using K2Bridge.Models.Request.Queries.LuceneNet;
    using K2Bridge.Visitors;
    using K2Bridge.Visitors.LuceneNet;
    using NUnit.Framework;
    using Tests;

    [TestFixture]
    public class TestLuceneWildcardVisitor
    {
        [TestCase]
        public void TestNullWildcardQuery()
        {
            var visitor = new LuceneVisitor();
            Assert.That(
                () => visitor.Visit((LuceneWildcardQuery)null),
                Throws.TypeOf<ArgumentNullException>());
        }

        [TestCase]
        public void TestInvalidWildcardQuery()
        {
            var wildcardQuery = new LuceneWildcardQuery
            {
                LuceneQuery = null,
            };
            var visitor = new LuceneVisitor();
            Assert.That(
                () => visitor.Visit(wildcardQuery),
                Throws.TypeOf<IllegalClauseException>());
        }

        [TestCase(ExpectedResult = "* matches regex \"Lo[.\\\\S]*d[.\\\\S]n\"")]
        public string Visit_ValidWildcardQuery_Success()
        {
            var wildcardQuery = new LuceneWildcardQuery
            {
                LuceneQuery =
                new Lucene.Net.Search.WildcardQuery(
                    new Lucene.Net.Index.Term("*", "Lo*d?n")),
            };

            var luceneVisitor = new LuceneVisitor();
            luceneVisitor.Visit(wildcardQuery);

            var es = wildcardQuery.ESQuery;
            Assert.NotNull(es);

            var visitor = new ElasticSearchDSLVisitor(LazySchemaRetrieverMock.CreateMockSchemaRetriever());
            visitor.Visit((QueryStringClause)es);

            return ((QueryStringClause)es).KustoQL;
        }
    }
}
