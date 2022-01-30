// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2Bridge.Tests.UnitTests.Visitors;

using System;
using K2Bridge.Models.Request.Queries;
using K2Bridge.Visitors;
using NUnit.Framework;

[TestFixture]
public class MatchPhraseVisitorTests
{
    [TestCase(ExpectedResult = "['MyField'] == \"MyPhrase\"")]
    public string MatchPhraseVisit_WithValidClause_ReturnsEquals()
    {
        var matchPhraseClause = CreateMatchPhraseClause("MyField", "MyPhrase");

        return VisitQuery(matchPhraseClause);
    }

    [TestCase(ExpectedResult = "['MyField'] == 3")]
    public string MatchPhraseVisit_WithValidIntClause_ReturnsEquals()
    {
        var matchPhraseClause = CreateMatchPhraseClause("MyField", 3);

        return VisitQuery(matchPhraseClause);
    }

    [TestCase(ExpectedResult = "['MyField'] == 3.5")]
    public string MatchPhraseVisit_WithValidDoubleClause_ReturnsEquals()
    {
        var matchPhraseClause = CreateMatchPhraseClause("MyField", 3.5);

        return VisitQuery(matchPhraseClause);
    }

    [TestCase(ExpectedResult = "['MyField'] == todatetime(\"2017-01-02T13:45:23.1330000Z\")")]
    public string MatchPhraseVisit_WithValidDateClause_ReturnsEquals()
    {
        var matchPhraseClause = CreateMatchPhraseClause("MyField", new DateTime(2017, 1, 2, 13, 45, 23, 133, DateTimeKind.Utc));

        return VisitQuery(matchPhraseClause);
    }

    [TestCase(ExpectedResult = "['MyField'] == \"test {test} test\"")]
    public string MatchPhraseVisit_WithBrackets_ReturnsEquals()
    {
        var matchPhraseClause = CreateMatchPhraseClause("MyField", "test {test} test");

        return VisitQuery(matchPhraseClause);
    }

    [TestCase(ExpectedResult = @"['MyField'] == ""test \""test\"" test""")]
    public string MatchPhraseVisit_WithQuotes_ReturnsEquals()
    {
        var matchPhraseClause = CreateMatchPhraseClause("MyField", @"test ""test"" test");

        return VisitQuery(matchPhraseClause);
    }

    [TestCase(ExpectedResult = @"['MyField'] == ""test \\ \""test\"" \\ test""")]
    public string MatchPhraseVisit_WithSlashes_ReturnsEquals()
    {
        var matchPhraseClause = CreateMatchPhraseClause("MyField", @"test \ ""test"" \ test");

        return VisitQuery(matchPhraseClause);
    }

    [TestCase(ExpectedResult = "['MyField'] == \"\"")]
    public string MatchPhraseVisit_WithoutClause_ReturnsEqualsEmpty()
    {
        var matchPhraseClause = CreateMatchPhraseClause("MyField", null);

        return VisitQuery(matchPhraseClause);
    }

    [TestCase]
    public void MatchPhraseVisit_WithInvalidClause_ReturnsEqualsEmpty()
    {
        var matchPhraseClause = CreateMatchPhraseClause(null, "myPhrase");

        Assert.Throws(typeof(IllegalClauseException), () => VisitQuery(matchPhraseClause));
    }

    private static MatchPhraseClause CreateMatchPhraseClause(string fieldName, object phrase)
    {
        return new MatchPhraseClause
        {
            FieldName = fieldName,
            Phrase = phrase,
        };
    }

    private static string VisitQuery(MatchPhraseClause clause)
    {
        var visitor = new ElasticSearchDSLVisitor(SchemaRetrieverMock.CreateMockSchemaRetriever());
        visitor.Visit(clause);
        return clause.KustoQL;
    }
}
