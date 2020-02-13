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
    public class LuceneWildcardVisitorTests
    {
        [TestCase]
        public void Visit_WithNullWildcardQuery_ThrowsException()
        {
            var visitor = new LuceneVisitor();
            Assert.That(
                () => visitor.Visit((LuceneWildcardQuery)null),
                Throws.TypeOf<ArgumentNullException>());
        }

        [TestCase]
        public void Visit_WithInvalidWildcardQuery_ThrowsException()
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
        public string Visit_WithValidWildcardQuery_ReturnsValidResponse()
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

            var visitor = new ElasticSearchDSLVisitor(SchemaRetrieverMock.CreateMockSchemaRetriever());
            visitor.Visit((QueryStringClause)es);

            return ((QueryStringClause)es).KustoQL;
        }
    }
}
