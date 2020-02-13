// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace UnitTests.K2Bridge.Visitors
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using global::K2Bridge;
    using global::K2Bridge.Models.Request;
    using global::K2Bridge.Visitors;
    using Microsoft.Extensions.Logging;
    using Moq;
    using NUnit.Framework;

    [TestFixture]
    public class ElasticQueryTranslatorTests
    {
        private const string DATADIR = "../../../Data";

        private const string INDEX = "{\"index\":\"myIndex\"}";

        private Mock<ILogger<ElasticQueryTranslator>> mockLogger;
        private Mock<IVisitor> mockVisitor;
        private ElasticQueryTranslator elasticQueryTranslator;

        [SetUp]
        public void Init()
        {
            mockLogger = new Mock<ILogger<ElasticQueryTranslator>>();
            mockVisitor = new Mock<IVisitor>();

            mockVisitor.Setup(visitor => visitor.Visit(It.IsNotNull<ElasticSearchDSL>()))
                .Callback<ElasticSearchDSL>((dsl) => dsl.KustoQL = "some kql from mock visitor");

            elasticQueryTranslator = new ElasticQueryTranslator(
                mockVisitor.Object,
                mockLogger.Object);
        }

        [TestCase(ExpectedResult = "some kql from mock visitor")]
        public string Translate_WithValidInput_ReturnsValidResponse()
        {
            var query = File.ReadAllText($"{DATADIR}/simple_k2_query.json");

            // Should succeed as all arguments are valid. the result is just a simple
            // hard coded mock
            var querydata = elasticQueryTranslator.Translate(INDEX, query);
            return querydata.QueryCommandText;
        }

        [TestCase]
        public void Translate_WithInvalidHeader_ThrowsException()
        {
            var query = File.ReadAllText($"{DATADIR}/simple_k2_query.json");

            // will fail as empty header is not valid
            Assert.That(
                () => elasticQueryTranslator.Translate(string.Empty, query),
                Throws.TypeOf<ArgumentException>());

            // will fail as a header without 'index' is not valid
            Assert.That(
                () => elasticQueryTranslator.Translate("{\"notindex\":\"myIndex\"}", query),
                Throws.TypeOf<KeyNotFoundException>());
        }

        [TestCase]
        public void Translate_WithInvalidQuery_ThrowsException()
        {
            var query = File.ReadAllText($"{DATADIR}/invalid_k2_query_no_query.json");

            // will fail as query is not valid (missing query)
            Assert.That(
                () => elasticQueryTranslator.Translate(INDEX, query),
                Throws.TypeOf<ArgumentNullException>());

            query = File.ReadAllText($"{DATADIR}/invalid_k2_query_no_bool.json");

            // will fail as query is not valid (missing query.bool)
            Assert.That(
                () => elasticQueryTranslator.Translate(INDEX, query),
                Throws.TypeOf<ArgumentNullException>());
        }

        [TestCase]
        public void Translate_SortIsNull_NoException()
        {
            var query = File.ReadAllText($"{DATADIR}/query_no_sort.json");

            Assert.That(() => elasticQueryTranslator.Translate(INDEX, query), Throws.Nothing);
        }
    }
}