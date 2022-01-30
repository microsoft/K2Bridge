// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2Bridge.Tests.UnitTests.Visitors.LuceneNet;

using System;
using K2Bridge.Models.Request.Queries;
using K2Bridge.Models.Request.Queries.LuceneNet;
using K2Bridge.Tests.UnitTests.Visitors;
using K2Bridge.Visitors;
using K2Bridge.Visitors.LuceneNet;
using NUnit.Framework;

[TestFixture]
public class LuceneRangeVisitorTests
{
    [TestCase]
    public void Visit_WithNullPrefixQuery_ThrowsException()
    {
        var visitor = new LuceneVisitor();
        Assert.That(
            () => visitor.Visit((LuceneRangeQuery)null),
            Throws.TypeOf<ArgumentNullException>());
    }

    [TestCase]
    public void Visit_WithInvalidRangeQuery_ThrowsException()
    {
        var rangeQuery = new LuceneRangeQuery
        {
            LuceneQuery = null,
        };
        var visitor = new LuceneVisitor();
        Assert.That(
            () => visitor.Visit(rangeQuery),
            Throws.TypeOf<IllegalClauseException>());
    }

    [TestCase(ExpectedResult = "['days'] >= 2 and ['days'] <= 6")]
    public string Visit_WithValidRangeQuery_ReturnsValidResponse()
    {
        var rangeQuery = new LuceneRangeQuery
        {
            LuceneQuery =
            new Lucene.Net.Search.TermRangeQuery(
                "days",
                "2",
                "6",
                true,
                true),
        };

        var luceneVisitor = new LuceneVisitor();
        rangeQuery.Accept(luceneVisitor);

        var es = rangeQuery.ESQuery;
        Assert.NotNull(es);

        var visitor = VisitorTestsUtils.CreateAndVisitRootVisitor("days", "long");
        visitor.Visit((RangeClause)es);

        return ((RangeClause)es).KustoQL;
    }
}
