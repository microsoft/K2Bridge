// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

using K2Bridge.Models.Request.Queries;
using Lucene.Net.QueryParsers;
using NUnit.Framework;

namespace K2Bridge.Tests.UnitTests.Visitors;

[TestFixture]
public class QueryStringClauseVisitorTests
{
    [TestCase(ExpectedResult = "* has \"myPhrase\"")]
    public string Visit_WithSimplePhrase_ReturnsHasResponse()
    {
        var queryClause = CreateQueryStringClause("myPhrase", true);

        return VisitQuery(queryClause);
    }

    [TestCase(ExpectedResult = "(* has \"myPhrase\") and (* has \"herPhrase\")")]
    public string Visit_WithMultiplePhrase_ReturnsHasAndResponse()
    {
        var queryClause = CreateQueryStringClause("myPhrase AND herPhrase", true);

        return VisitQuery(queryClause);
    }

    [TestCase(ExpectedResult = "(* has \"myPhrase\") or (* has \"herPhrase\")")]
    public string Visit_WithMultipleOrPhrase_ReturnsHasOrResponse()
    {
        var queryClause = CreateQueryStringClause("myPhrase OR herPhrase", true);

        return VisitQuery(queryClause);
    }

    [TestCase(ExpectedResult = "(* !has \"myPhrase\") and (* !has \"herPhrase\")")]
    public string Visit_WithMultipleNotPhrase_ReturnsHasAndNotResponse()
    {
        var queryClause = CreateQueryStringClause("NOT myPhrase AND NOT herPhrase", true);

        return VisitQuery(queryClause);
    }

    [TestCase(ExpectedResult = "(* has \"Dogs\") and (* contains \"My cats\")")]
    public string Visit_WithQuotationPhrase_ReturnsContainsResponse()
    {
        var queryClause = CreateQueryStringClause("Dogs AND \"My cats\"", true);

        return VisitQuery(queryClause);
    }

    [TestCase(ExpectedResult = "(* has \"shouldBeFirst\") and (* has \"shouldBeSecond\") and (['this'].['that'] has \"shouldBeThird\") and (['that'].['this'] contains \"should be fourth\")")]
    public string Visit_WithDynamicFields_ReturnsSortedResponse()
    {
        var queryClause = CreateQueryStringClause("this.that:shouldBeThird AND shouldBeFirst AND that.this:\"should be fourth\" AND shouldBeSecond", true);

        return VisitQuery(queryClause);
    }

    [TestCase(ExpectedResult = "(* has \"Tokyo\") and (* contains \"Haneda International\") or ((* has \"A\") and (* contains \"b c\"))")]
    public string Visit_WithMultipleQuotationPhrase_ReturnsAndContainsResponse()
    {
        var queryClause = CreateQueryStringClause("Tokyo AND \"Haneda International\" OR (A AND \"b c\")", true);

        return VisitQuery(queryClause);
    }

    [TestCase(ExpectedResult = @"* has ""\""Get""")]
    public string Visit_WithEscapedQuotationPhrase_ReturnsAndContainsResponse()
    {
        var queryClause = CreateQueryStringClause(@"\""Get", false);

        return VisitQuery(queryClause);
    }

    [TestCase(ExpectedResult = @"* has ""{\""Get}""")]
    public string Visit_WithBrackets_ReturnsAndContainsResponse()
    {
        var queryClause = CreateQueryStringClause(@"\{\""Get\}", false);

        return VisitQuery(queryClause);
    }

    [TestCase(ExpectedResult = @"* has ""Get""")]
    public string Visit_WithQuoted_OpensQuoteReturnsAndContainsResponse()
    {
        var queryClause = CreateQueryStringClause(@"""Get""", false);

        return VisitQuery(queryClause);
    }

    [TestCase]
    public void Visit_WithIncorrectlyQuoted_Throws()
    {
        Assert.Throws<ParseException>(() =>
        {
            var queryClause = CreateQueryStringClause(@"""Get", false);

            VisitQuery(queryClause);
        });
    }

    [TestCase(ExpectedResult = "* has \"\\\\dev\\\\kusto\\\\K2Bridge\"")]
    public string Visit_WithBreakQuotePhrase_ReturnsAndContainsResponse()
    {
        var queryClause = CreateQueryStringClause(@"\\dev\\kusto\\K2Bridge", false);

        return VisitQuery(queryClause);
    }

    [TestCase(ExpectedResult = null)]
    public string Visit_WithEmptyQuotePhrase_ReturnsAndContainsResponse()
    {
        var queryClause = CreateQueryStringClause("\"\"", false);

        return VisitQuery(queryClause);
    }

    private static string VisitQuery(QueryStringClause queryStringClause)
    {
        var visitor = VisitorTestsUtils.CreateAndVisitRootVisitor();
        visitor.Visit(queryStringClause);
        return queryStringClause.KustoQL;
    }

    private static QueryStringClause CreateQueryStringClause(string phrase, bool wildcard)
    {
        return new QueryStringClause
        {
            Phrase = phrase,
            Wildcard = wildcard,
            Default = "*",
        };
    }
}
