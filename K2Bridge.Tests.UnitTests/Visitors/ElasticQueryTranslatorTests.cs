// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2Bridge.Tests.UnitTests.Visitors;

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
public class ElasticQueryTranslatorTests
{
    private const string DATADIR = "../../../Data";

    private const string INDEX = "{\"index\":\"myIndex\"}";

    private const string MustField = "MyTerm";
    private const string FilterField = "test";

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
        var queryData = elasticQueryTranslator.TranslateQuery(INDEX, query);

        Assert.NotNull(queryData);
        return queryData.Value.QueryCommandText;
    }

    [TestCase(ExpectedResult = "some kql from mock visitor")]
    public string Translate_WithValidInput_ReturnsValidHighlights()
    {
        var query = File.ReadAllText($"{DATADIR}/simple_k2_query.json");

        var queryData = elasticQueryTranslator.TranslateQuery(INDEX, query);
        Assert.NotNull(queryData);

        Assert.AreEqual(queryData.Value.HighlightText["*"], MustField);
        Assert.AreEqual(queryData.Value.HighlightText[FilterField], "5");

        return queryData.Value.QueryCommandText;
    }

    [TestCase]
    public void Translate_WithInvalidHeader_ThrowsException()
    {
        var query = File.ReadAllText($"{DATADIR}/simple_k2_query.json");

        // will fail as empty header is not valid
        Assert.That(
            () => elasticQueryTranslator.TranslateQuery(string.Empty, query),
            Throws.TypeOf<ArgumentException>());

        // will fail in translation as a header without 'index' is not valid
        Assert.That(
            () => elasticQueryTranslator.TranslateQuery("{\"notindex\":\"myIndex\"}", query),
            Throws.TypeOf<TranslateException>());
    }

    [TestCase]
    public void Translate_WithInvalidQuery_ReturnsNull()
    {
        var query = File.ReadAllText($"{DATADIR}/invalid_k2_query_no_query.json");


        Assert.AreEqual(null, elasticQueryTranslator.TranslateQuery(INDEX, query));

        query = File.ReadAllText($"{DATADIR}/invalid_k2_query_no_bool.json");

        // will fail as query is not valid (missing query.bool)
        Assert.That(
            () => elasticQueryTranslator.TranslateQuery(INDEX, query),
            Throws.TypeOf<TranslateException>());
    }

    [TestCase]
    public void Translate_SortIsNull_NoException()
    {
        var query = File.ReadAllText($"{DATADIR}/query_no_sort.json");

        Assert.That(() => elasticQueryTranslator.TranslateQuery(INDEX, query), Throws.Nothing);
    }

    [TestCaseSource(nameof(QuotedIndexTestCases))]
    public void Translate_WithQuotedIndex_KeepsNamesInsideLiterals(string index, string defaultDatabase, string source, bool singleDocument)
    {
        var visitor = new ElasticSearchDSLVisitor(SchemaRetrieverMock.CreateMockSchemaRetriever(), defaultDatabase);
        var translator = new ElasticQueryTranslator(visitor, mockLogger.Object);
        var header = System.Text.Json.JsonSerializer.Serialize(new { index });
        var query = CreateQuery(singleDocument);

        var result = translator.TranslateQuery(header, query);

        var expected = singleDocument
            ? $"{source} | where _id == \"document-id\" | as hits;"
            : $"let _data = {source} ;\n(_data | count | as hitsTotal);\n(_data | limit 0 | as hits)";
        Assert.That(result.HasValue, Is.True);
        Assert.That(result.Value.QueryCommandText, Is.EqualTo(expected));
        Assert.That(result.Value.IndexName, Is.EqualTo(index));
    }

    [TestCase(null, false)]
    [TestCase(null, true)]
    [TestCase("", false)]
    [TestCase("", true)]
    public void Translate_WithMissingIndex_RejectsQuery(string index, bool singleDocument)
    {
        var visitor = new ElasticSearchDSLVisitor(SchemaRetrieverMock.CreateMockSchemaRetriever(), "configuredDb");
        var translator = new ElasticQueryTranslator(visitor, mockLogger.Object);
        var header = System.Text.Json.JsonSerializer.Serialize(new { index });
        var query = CreateQuery(singleDocument);

        Assert.Throws<TranslateException>(() => translator.TranslateQuery(header, query));
    }

    private static string CreateQuery(bool singleDocument)
    {
        return singleDocument
            ? "{\"query\":{\"ids\":{\"values\":[\"document-id\"]}}}"
            : "{\"query\":{\"bool\":{\"must\":[],\"filter\":[]}},\"size\":0}";
    }

    private static IEnumerable<TestCaseData> QuotedIndexTestCases()
    {
        var cases = new[]
        {
            ("defaultdb:logs", "database(\"defaultdb\").['logs']"),
            ("logs", "database(\"configuredDb\").['logs']"),
            (":logs", "database(\"\").['logs']"),
            ("prod-db:logs.2026-11", "database(\"prod-db\").['logs.2026-11']"),
            ("prod db:logs archive", "database(\"prod db\").['logs archive']"),
            ("defaultdb:logs:archive", "database(\"defaultdb\").['logs:archive']"),
            ("defaultdb:logs_2026", "database(\"defaultdb\").['logs_2026']"),
            (
                "defaultdb:logs'] | union database(\"other\").['secret",
                "database(\"defaultdb\").['logs\\'] | union database(\"other\").[\\'secret']"),
            (
                "de\"fault\\db:lo'gs\\",
                "database(\"de\\\"fault\\\\db\").['lo\\'gs\\\\']"),
            (
                "db\nname:logs\r\nnext",
                "database(\"db\\u000aname\").['logs\\u000d\\u000anext']"),
            ("defaultdb:where", "database(\"defaultdb\").['where']"),
            ("defaultdb:123logs", "database(\"defaultdb\").['123logs']"),
            ("defaultdb:logs[archive]", "database(\"defaultdb\").['logs[archive]']"),
            ("defaultdb:logs\\u0027", "database(\"defaultdb\").['logs\\\\u0027']"),
            ("defaultdb:logs\\\\", "database(\"defaultdb\").['logs\\\\\\\\']"),
            (
                "db\u2028name:logs\u2029next",
                "database(\"db\\u2028name\").['logs\\u2029next']"),
            (
                "\u6570\u636e:\u65e5\u5fd7_\ud83d\ude00",
                "database(\"\u6570\u636e\").['\u65e5\u5fd7_\ud83d\ude00']"),
        };

        foreach (var (index, source) in cases)
        {
            yield return new TestCaseData(index, "configuredDb", source, false);
            yield return new TestCaseData(index, "configuredDb", source, true);
        }

        var defaultDatabases = new[]
        {
            ("prod\"db\\", "database(\"prod\\\"db\\\\\").['logs']"),
            ("db\\u0022", "database(\"db\\\\u0022\").['logs']"),
            ("db\nname", "database(\"db\\u000aname\").['logs']"),
        };
        foreach (var (database, source) in defaultDatabases)
        {
            yield return new TestCaseData("logs", database, source, false);
            yield return new TestCaseData("logs", database, source, true);
        }
    }
}
