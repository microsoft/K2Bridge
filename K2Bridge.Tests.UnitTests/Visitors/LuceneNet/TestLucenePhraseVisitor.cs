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
    public class TestLucenePhraseVisitor
    {
        [TestCase]
        public void Visit_NullPrefixQuery_Throws()
        {
            var visitor = new LuceneVisitor();
            Assert.That(
                () => visitor.Visit((LucenePhraseQuery)null),
                Throws.TypeOf<ArgumentNullException>());
        }

        [TestCase]
        public void Visit_InvalidPhraseQuery_Throws()
        {
            var phraseQuery = new LucenePhraseQuery
            {
                LuceneQuery = null,
            };
            var visitor = new LuceneVisitor();
            Assert.That(
                () => visitor.Visit(phraseQuery),
                Throws.TypeOf<IllegalClauseException>());
        }

        [TestCase(ExpectedResult = "City contains \"TelAviv\"")]
        public string Visit_ValidTermPhraseQuery_Success()
        {
            var query = new Lucene.Net.Search.PhraseQuery();
            query.Add(new Lucene.Net.Index.Term("City", "TelAviv"));

            var phraseQuery = new LucenePhraseQuery
            {
                LuceneQuery = query,
            };

            var luceneVisitor = new LuceneVisitor();
            phraseQuery.Accept(luceneVisitor);

            var es = phraseQuery.ESQuery;
            Assert.NotNull(es);

            var visitor = new ElasticSearchDSLVisitor(LazySchemaRetrieverMock.CreateMockSchemaRetriever());
            visitor.Visit((QueryStringClause)es);

            return ((QueryStringClause)es).KustoQL;
        }

        [TestCase(ExpectedResult = "City has \"TelAviv\"")]
        public string Visit_ValidTermQuery_Success()
        {
            var query = new Lucene.Net.Search.TermQuery(new Lucene.Net.Index.Term("City", "TelAviv"));

            var phraseQuery = new LuceneTermQuery
            {
                LuceneQuery = query,
            };

            var luceneVisitor = new LuceneVisitor();
            phraseQuery.Accept(luceneVisitor);

            var es = phraseQuery.ESQuery;
            Assert.NotNull(es);

            var visitor = new ElasticSearchDSLVisitor(LazySchemaRetrieverMock.CreateMockSchemaRetriever());
            visitor.Visit((QueryStringClause)es);

            return ((QueryStringClause)es).KustoQL;
        }
    }
}
