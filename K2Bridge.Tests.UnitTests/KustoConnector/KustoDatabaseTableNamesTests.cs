// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace Tests.KustoConnector
{
    using K2Bridge.Models;
    using NUnit.Framework;

    [TestFixture]
    public class KustoDatabaseTableNamesTests
    {
        private static object[] badInputCases = {
            new string[] { string.Empty },
            new string[] { null },
        };

        private static object[] noColonCases = {
            new string[] { "table.name" },
            new string[] { "tableName" },
        };

        private static object[] colonEmptyStringCases = {
            new TestCaseData(":").Returns((string.Empty, string.Empty)),
            new TestCaseData("database:").Returns(("database", string.Empty)),
            new TestCaseData(":table").Returns((string.Empty, "table")),
        };

        private static object[] correctStringCases = {
            new TestCaseData("database:tablename").Returns(("database", "tablename")),
            new TestCaseData("database:table:name").Returns(("database", "table:name")), // multiple colons output as database name
        };

        private static object[] defaultDatabaseTests = {
            new TestCaseData("tablename", "database").Returns(("database", "tablename")),
            new TestCaseData("database:tablename", "notdatabase").Returns(("database", "tablename")),
            new TestCaseData("database:tablename", string.Empty).Returns(("database", "tablename")),
            new TestCaseData(":tablename", string.Empty).Returns((string.Empty, "tablename")),
            new TestCaseData(":tablename", "notdatabase").Returns((string.Empty, "tablename")),
        };

        [TestCaseSource("badInputCases")]
        public void ExceptionOnEmptyInputIndexName(string indexName)
        {
            try
            {
                var (databaseName, tableName) = KustoDatabaseTableNames.FromElasticIndexName(indexName, string.Empty);
            }
            catch
            {
                return;
            }

            Assert.Fail($"can not retrieve kusto database and table names for malformed indexName: {indexName}");
        }

        [TestCaseSource("noColonCases")]
        public void HanldesNoColonInInputIndexName(string indexName)
        {
            var (databaseName, tableName) = KustoDatabaseTableNames.FromElasticIndexName(indexName, string.Empty);
            Assert.AreEqual(string.Empty, databaseName, $"database name should be empty for input {indexName}");
        }

        [TestCaseSource("colonEmptyStringCases")]
        public (string, string) HanldesColonEmptyStringInInputIndexName(string indexName)
        {
            return KustoDatabaseTableNames.FromElasticIndexName(indexName, string.Empty);
        }

        [TestCaseSource("correctStringCases")]
        public (string, string) SetsTableNameAndDatabaseNameOnCorrectInputIndexName(string indexName)
        {
            return KustoDatabaseTableNames.FromElasticIndexName(indexName, string.Empty);
        }

        [TestCaseSource("defaultDatabaseTests")]
        public (string, string) SetsDefaultDatabaseOnCorrectInputIndexName(string indexName, string defaultTableName)
        {
            return KustoDatabaseTableNames.FromElasticIndexName(indexName, defaultTableName);
        }
    }
}