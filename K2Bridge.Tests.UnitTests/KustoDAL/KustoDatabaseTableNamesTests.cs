// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2Bridge.Tests.UnitTests.KustoDAL;

using System;
using K2Bridge.Utils;
using NUnit.Framework;

[TestFixture]
public class KustoDatabaseTableNamesTests
{
    private static readonly object[] BadInputTestCases = {
            new TestCaseData(string.Empty) { TestName = "FromElasticIndexName_WithEmptyString_ThrowsException" },
            new TestCaseData(null) { TestName = "FromElasticIndexName_WithNullString_ThrowsException" },
        };

    private static readonly object[] NoColonTestCases = {
            new TestCaseData("table.name") { TestName = "FromElasticIndexName_WithoutDatabaseNameWithDot_ReturnsValidResult" },
            new TestCaseData("tableName") { TestName = "FromElasticIndexName_WithoutDatabaseName_ReturnsValidResult" },
        };

    private static readonly object[] ColonEmptyStringTestCases = {
            new TestCaseData(":") { TestName = "FromElasticIndexName_WithEmptyDatabaseAndTableNames_ReturnsEmptyNames" }.Returns((string.Empty, string.Empty)),
            new TestCaseData("database:") { TestName = "FromElasticIndexName_WithEmptyDatabase_ReturnsEmptyDatabase" }.Returns(("database", string.Empty)),
            new TestCaseData(":table") { TestName = "FromElasticIndexName_WithEmptyTableName_ReturnsEmptyTableName" }.Returns((string.Empty, "table")),
        };

    private static readonly object[] CorrectStringTestCases = {
            new TestCaseData("database:tablename") { TestName = "FromElasticIndexName_WithDatabaseAndTableNames_ReturnsNames" }.Returns(("database", "tablename")),
            new TestCaseData("database:table:name") { TestName = "FromElasticIndexName_WithMultipleColons_ReturnsFirstTwo" }.Returns(("database", "table:name")), // multiple colons output as database name
        };

    private static readonly object[] DefaultDatabaseTestCases = {
            new TestCaseData("tablename", "database") { TestName = "FromElasticIndexName_WithNoColon_ReturnsDefaultDatabase" }.Returns(("database", "tablename")),
            new TestCaseData("database:tablename", "notdatabase") { TestName = "FromElasticIndexName_WithColon_ReturnsQueryDatabase" }.Returns(("database", "tablename")),
            new TestCaseData("database:tablename", string.Empty) { TestName = "FromElasticIndexName_WithColonAndEmptyDefault_ReturnsQueryDatabase" }.Returns(("database", "tablename")),
            new TestCaseData(":tablename", string.Empty) { TestName = "FromElasticIndexName_WithEmptyDatabaseAndColonAndEmptyDefault_ReturnsEmptyQueryDatabase" }.Returns((string.Empty, "tablename")),
            new TestCaseData(":tablename", "notdatabase") { TestName = "FromElasticIndexName_WithEmptyDatabaseAndColon_ReturnsEmptyQueryDatabase" }.Returns((string.Empty, "tablename")),
        };

    [TestCaseSource(nameof(BadInputTestCases))]
    public void ExceptionOnEmptyInputIndexName(string indexName)
    {
        try
        {
            var (databaseName, tableName) = KustoDatabaseTableNames.FromElasticIndexName(indexName, string.Empty);
        }
        catch (ArgumentException)
        {
            return;
        }

        Assert.Fail($"can not retrieve kusto database and table names for malformed indexName: {indexName}");
    }

    [TestCaseSource(nameof(NoColonTestCases))]
    public void HanldesNoColonInInputIndexName(string indexName)
    {
        var (databaseName, _) = KustoDatabaseTableNames.FromElasticIndexName(indexName, string.Empty);
        Assert.AreEqual(string.Empty, databaseName, $"database name should be empty for input {indexName}");
    }

    [TestCaseSource(nameof(ColonEmptyStringTestCases))]
    public (string, string) HanldesColonEmptyStringInInputIndexName(string indexName)
    {
        return KustoDatabaseTableNames.FromElasticIndexName(indexName, string.Empty);
    }

    [TestCaseSource(nameof(CorrectStringTestCases))]
    public (string, string) SetsTableNameAndDatabaseNameOnCorrectInputIndexName(string indexName)
    {
        return KustoDatabaseTableNames.FromElasticIndexName(indexName, string.Empty);
    }

    [TestCaseSource(nameof(DefaultDatabaseTestCases))]
    public (string, string) SetsDefaultDatabaseOnCorrectInputIndexName(string indexName, string defaultTableName)
    {
        return KustoDatabaseTableNames.FromElasticIndexName(indexName, defaultTableName);
    }
}
