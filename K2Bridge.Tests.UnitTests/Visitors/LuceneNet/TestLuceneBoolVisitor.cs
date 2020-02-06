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
    public class TestLuceneBoolVisitor
    {
        [TestCase]
        public void Visit_NullBoolQuery_Throws()
        {
            var visitor = new LuceneVisitor();
            Assert.That(
                () => visitor.Visit((LuceneBoolQuery)null),
                Throws.TypeOf<ArgumentNullException>());
        }

        [TestCase]
        public void Visit_InvalidBoolQuery_Throws()
        {
            var boolQuery = new LuceneBoolQuery
            {
                LuceneQuery = null,
            };
            var visitor = new LuceneVisitor();
            Assert.That(
                () => visitor.Visit(boolQuery),
                Throws.TypeOf<IllegalClauseException>());
        }

        [TestCase(ExpectedResult = "not (* matches regex \"Lo[.\\\\S]*d[.\\\\S]n\")")]
        public string Visit_ValidBoolQuery_Success()
        {
            var luceneNetBoolQuery = new Lucene.Net.Search.BooleanQuery();
            luceneNetBoolQuery.Clauses.Add(
                new Lucene.Net.Search.BooleanClause(
                    new Lucene.Net.Search.WildcardQuery(
                        new Lucene.Net.Index.Term("*", "Lo*d?n")),
                    Lucene.Net.Search.Occur.MUST_NOT));

            var boolQuery = new LuceneBoolQuery
            {
                LuceneQuery = luceneNetBoolQuery,
            };

            var luceneVisitor = new LuceneVisitor();
            luceneVisitor.Visit(boolQuery);

            var es = boolQuery.ESQuery;
            Assert.NotNull(es);

            var visitor = new ElasticSearchDSLVisitor(LazySchemaRetrieverMock.CreateMockSchemaRetriever());
            visitor.Visit((BoolQuery)es);

            return ((BoolQuery)es).KustoQL;
        }
    }
}
