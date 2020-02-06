// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2BridgeUnitTests.Visitors.LuceneNet
{
    using System;
    using K2Bridge;
    using K2Bridge.Models.Request.Queries;
    using K2Bridge.Models.Request.Queries.LuceneNet;
    using K2Bridge.Visitors;
    using K2Bridge.Visitors.LuceneNet;
    using NUnit.Framework;
    using Tests;

    [TestFixture]
    public class TestLucenePrefixVisitor
    {
        [TestCase]
        public void Visit_NullPrefixQuery_Fails()
        {
            var visitor = new LuceneVisitor();
            Assert.That(
                () => visitor.Visit((LucenePrefixQuery)null),
                Throws.TypeOf<ArgumentNullException>());
        }

        [TestCase]
        public void Visit_InvalidPrefixQuery_Throws()
        {
            var prefixQuery = new LucenePrefixQuery
            {
                LuceneQuery = null,
            };
            var visitor = new LuceneVisitor();
            Assert.That(
                () => visitor.Visit(prefixQuery),
                Throws.TypeOf<IllegalClauseException>());
        }

        [TestCase(ExpectedResult = "* hasprefix_cs \"Kfar-Sa*\"")]
        public string Visit_ValidWildcardQuery_Success()
        {
            var prefixQuery = new LucenePrefixQuery
            {
                LuceneQuery =
                new Lucene.Net.Search.PrefixQuery(
                    new Lucene.Net.Index.Term("*", "Kfar-Sa*")),
            };

            var luceneVisitor = new LuceneVisitor();
            luceneVisitor.Visit(prefixQuery);

            var es = prefixQuery.ESQuery;
            Assert.NotNull(es);

            var visitor = new ElasticSearchDSLVisitor(LazySchemaRetrieverMock.CreateMockSchemaRetriever());
            visitor.Visit((QueryStringClause)es);

            return ((QueryStringClause)es).KustoQL;
        }
    }
}
