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

    [TestFixture]
    public class LuceneMatchAllDocsVisitorTests
    {
        [TestCase]
        public void Visit_WithNullQuery_ThrowsException()
        {
            var visitor = new LuceneVisitor();
            Assert.That(
                () => visitor.Visit((LuceneMatchAllDocsQuery)null),
                Throws.TypeOf<ArgumentNullException>());
        }

        [TestCase]
        public void Visit_WithInvalidQuery_ThrowsException()
        {
            var query = new LuceneMatchAllDocsQuery {
                LuceneQuery = null,
            };
            var visitor = new LuceneVisitor();
            Assert.That(
                () => visitor.Visit(query),
                Throws.TypeOf<IllegalClauseException>());
        }

        [TestCase(ExpectedResult = null)]
        public string Visit_WithValidQuery_ReturnsValidResponse()
        {
            var query = new LuceneMatchAllDocsQuery {
                LuceneQuery =
                    new Lucene.Net.Search.MatchAllDocsQuery(),
            };

            var luceneVisitor = new LuceneVisitor();
            luceneVisitor.Visit(query);

            var es = query.ESQuery;
            Assert.NotNull(es);

            var visitor = new ElasticSearchDSLVisitor(SchemaRetrieverMock.CreateMockSchemaRetriever());
            visitor.Visit((QueryStringClause)es);

            return ((QueryStringClause)es).KustoQL;
        }
    }
}
