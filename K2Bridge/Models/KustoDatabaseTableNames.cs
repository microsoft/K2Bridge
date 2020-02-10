// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2Bridge.Models
{
    using System;

    /// <summary>
    /// Static class to hanlde formatting of elastic index name to adx database
    /// and table names.
    /// </summary>
    internal static class KustoDatabaseTableNames
    {
        /// <summary>
        /// Returns adx database and table names from elastic indexname using
        /// ':' char separator.
        /// </summary>
        /// <param name="indexName">a colon separated string.</param>
        /// <returns>database and table names as tuple.</returns>

        /// <summary>
        /// Separator charachter between kusto database and table names to support
        /// the database level which does not exist in elastic indexes.
        /// </summary>
        public const char Separator = ':';

        /// <summary>
        /// Wildcard used by elastic to retrieve all indexes.
        /// </summary>
        public const string Wildcard = "*";

        /// <summary>
        /// Converts a string which represents an elastic index name to
        /// kusto pair of database and table names.
        /// </summary>
        /// <param name="indexName">Index name.</param>
        /// <param name="defaultDatabaseName">Default database name.</param>
        /// <returns>An object with the field caps.</returns>
        public static (string DatabaseName, string TableName) FromElasticIndexName(string indexName, string defaultDatabaseName)
        {
            Ensure.IsNotNullOrEmpty(indexName, nameof(indexName), "Input cannot be null");

            if (indexName.Equals(Wildcard, StringComparison.OrdinalIgnoreCase))
            {
                return (Wildcard, Wildcard);
            }

            var splitIndex = indexName.IndexOf(Separator, StringComparison.OrdinalIgnoreCase);
            if (splitIndex < 0)
            {
                return (defaultDatabaseName, indexName);
            }

            var databaseName = indexName.Substring(0, splitIndex);
            var tableName = indexName.Substring(splitIndex + 1, indexName.Length - splitIndex - 1);
            return (databaseName, tableName);
        }
    }
}