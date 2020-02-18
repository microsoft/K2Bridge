// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace UnitTests.K2Bridge.Visitors.LuceneNet
{
    using System;
    using global::K2Bridge.Models.Request.Queries;
    using global::K2Bridge.Models.Request.Queries.LuceneNet;
    using global::K2Bridge.Visitors;
    using global::K2Bridge.Visitors.LuceneNet;
    using NUnit.Framework;
    using UnitTests.K2Bridge.Visitors;

    [TestFixture]
    public class LucenePrefixVisitorTests
    {
        [TestCase]
        public void Visit_WithNullPrefixQuery_ThrowsException()
        {
            var visitor = new LuceneVisitor();
            Assert.That(
                () => visitor.Visit((LucenePrefixQuery)null),
                Throws.TypeOf<ArgumentNullException>());
        }

        [TestCase]
        public void Visit_WithInvalidPrefixQuery_ThrowsException()
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

        [TestCase(ExpectedResult = "* hasprefix \"Kfar-Sa*\"")]
        public string Visit_WithValidWildcardQuery_ReturnsValidReponse()
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

            var visitor = new ElasticSearchDSLVisitor(SchemaRetrieverMock.CreateMockSchemaRetriever());
            visitor.Visit((QueryStringClause)es);

            return ((QueryStringClause)es).KustoQL;
        }
    }
}
