// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2Bridge.Tests.UnitTests.Visitors;

using K2Bridge.Models.Request;
using K2Bridge.Visitors;
using NUnit.Framework;

[TestFixture]
public class SortClauseVisitorTests
{
    [TestCase(
        ExpectedResult = "",
        TestName = "Visit_WithUnderscoreInput_Ignores")]
    public string IgnoresClausesWithUnderscore()
    {
        var sortClause = new SortClause() { FieldName = "_wibble" };

        var visitor = new ElasticSearchDSLVisitor(SchemaRetrieverMock.CreateMockSchemaRetriever());
        visitor.Visit(sortClause);

        return sortClause.KustoQL;
    }

    [TestCase(
        ExpectedResult = "['wibble'] asc",
        TestName = "Visit_WithValidInput_ReturnsExpectedResult")]
    public string GeneratesClauseQuery()
    {
        var sortClause = new SortClause() { FieldName = "wibble", Order = "asc" };

        var visitor = new ElasticSearchDSLVisitor(SchemaRetrieverMock.CreateMockSchemaRetriever());
        visitor.Visit(sortClause);

        return sortClause.KustoQL;
    }
}
