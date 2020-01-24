// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2Bridge.Tests.UnitTests
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using K2Bridge;
    using K2Bridge.Models.Request;
    using K2Bridge.Visitors;
    using Microsoft.Extensions.Logging;
    using Moq;
    using NUnit.Framework;

    [TestFixture]
    public class TestElasticQueryTranslator
    {
        private const string DATADIR = "../../../Data";

        [TestCase(ExpectedResult = "some kql from mock visitor")]
        public string TestValidElasticQueryTranslator()
        {
            var query = File.ReadAllText($"{DATADIR}/simple_k2_query.json");

            var mockLogger = new Mock<ILogger<ElasticQueryTranslator>>();
            var mockVisitor = new Mock<IVisitor>();
            mockVisitor.Setup(
                visitor => visitor.Visit(It.IsNotNull<ElasticSearchDSL>()))
                .Callback<ElasticSearchDSL>((dsl) => dsl.KQL = "some kql from mock visitor");

            var eqt = new ElasticQueryTranslator(
                mockVisitor.Object,
                mockLogger.Object);

            // Should succeed as all arguments are valid. the result is just a simple
            // hard coded mock
            var querydata = eqt.Translate("{\"index\":\"myIndex\"}", query);
            return querydata.KQL;
        }

        [TestCase]
        public void TestInvalidHeader()
        {
            var query = File.ReadAllText($"{DATADIR}/simple_k2_query.json");

            var mockLogger = new Mock<ILogger<ElasticQueryTranslator>>();
            var mockVisitor = new Mock<IVisitor>();
            mockVisitor.Setup(
                visitor => visitor.Visit(It.IsNotNull<ElasticSearchDSL>()))
                .Callback<ElasticSearchDSL>((dsl) => dsl.KQL = "some kql from mock visitor");

            var eqt = new ElasticQueryTranslator(
                mockVisitor.Object,
                mockLogger.Object);

            // will fail as empty header is not valid
            Assert.That(
                () => eqt.Translate(string.Empty, query),
                Throws.TypeOf<ArgumentException>());

            // will fail as a header without 'index' is not valid
            Assert.That(
                () => eqt.Translate("{\"notindex\":\"myIndex\"}", query),
                Throws.TypeOf<KeyNotFoundException>());
        }

        [TestCase]
        public void TestInvalidQuery()
        {
            var mockLogger = new Mock<ILogger<ElasticQueryTranslator>>();
            var mockVisitor = new Mock<IVisitor>();
            mockVisitor.Setup(
                visitor => visitor.Visit(It.IsNotNull<ElasticSearchDSL>()))
                .Callback<ElasticSearchDSL>((dsl) => dsl.KQL = "some kql from mock visitor");

            var eqt = new ElasticQueryTranslator(
                mockVisitor.Object,
                mockLogger.Object);

            var query =
                File.ReadAllText($"{DATADIR}/invalid_k2_query_no_query.json");

            // will fail as query is not valid (missing query)
            Assert.That(
                () => eqt.Translate("{\"index\":\"myIndex\"}", query),
                Throws.TypeOf<ArgumentNullException>());

            query = File.ReadAllText($"{DATADIR}/invalid_k2_query_no_bool.json");

            // will fail as query is not valid (missing query.bool)
            Assert.That(
                () => eqt.Translate("{\"index\":\"myIndex\"}", query),
                Throws.TypeOf<ArgumentNullException>());
        }
    }
}
