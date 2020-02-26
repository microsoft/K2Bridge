// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace UnitTests.K2Bridge.Visitors.LuceneNet
{
    using System;
    using global::K2Bridge.Models.Request;
    using global::K2Bridge.Models.Request.Queries;
    using global::K2Bridge.Models.Request.Queries.LuceneNet;
    using global::K2Bridge.Tests.UnitTests.Visitors;
    using global::K2Bridge.Visitors;
    using global::K2Bridge.Visitors.LuceneNet;
    using NUnit.Framework;
    using UnitTests.K2Bridge.Visitors;

    [TestFixture]
    public class LucenePhraseVisitorTests
    {
        [TestCase]
        public void Visit_WithNullPrefixQuery_ThrowsException()
        {
            var visitor = new LuceneVisitor();
            Assert.That(
                () => visitor.Visit((LucenePhraseQuery)null),
                Throws.TypeOf<ArgumentNullException>());
        }

        [TestCase]
        public void Visit_WithInvalidPhraseQuery_ThrowsException()
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
        public string Visit_WithValidTermPhraseQuery_ReturnsValidReponse()
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

            var visitor = new ElasticSearchDSLVisitor(SchemaRetrieverMock.CreateMockSchemaRetriever());
            visitor.Visit((QueryStringClause)es);

            return ((QueryStringClause)es).KustoQL;
        }

        [TestCase(ExpectedResult = "City has \"TelAviv\"")]
        public string Visit_WithValidTermQuery_ReturnsValidReponse()
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

            var visitor = VisitorTestsUtils.CreateDefaultVisitor();
            visitor.Visit((QueryStringClause)es);

            return ((QueryStringClause)es).KustoQL;
        }
    }
}
