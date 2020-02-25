// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2Bridge.Factories
{
    using System;
    using System.Data;
    using K2Bridge.Models.Response.Metadata;

    /// <summary>
    /// Convertos methods for <see cref="FieldCapabilityElement"/>.
    /// </summary>
    public static class FieldCapabilityElementFactory
    {
        /// <summary>
        /// Creates a new instance of <see cref="FieldCapabilityElement"/>.
        /// </summary>
        /// <param name="record">IDataRecord.</param>
        /// <returns>FieldCapabilityElement.</returns>
        public static FieldCapabilityElement CreateFromDataRecord(IDataRecord record)
        {
            Ensure.IsNotNull(record, nameof(record));

            var columnName = record[(int)FieldCapabilityElementDataReaderMapping.ColumnName];
            var columnType = record[(int)FieldCapabilityElementDataReaderMapping.ColumnType];

            return new FieldCapabilityElement
            {
                Name = Convert.ToString(columnName),
                Type = ElasticTypeFromKustoType(Convert.ToString(columnType)),
            };
        }

        private static string ElasticTypeFromKustoType(string type)
        {
            return type switch
            {
                "System.Int32" => "integer",
                "System.Int64" => "long",
                "System.Single" => "float",
                "System.Double" => "double",
                "System.SByte" => "boolean",
                "System.Object" => "object",
                "System.String" => "keyword", // Elastic support text and keyword string types. Text is interpreted as something that can't be aggregated, hence we need to choose keyword.
                "System.DateTime" => "date",
                "System.Data.SqlTypes.SqlDecimal" => "double",
                "System.Guid" => "string",
                "System.TimeSpan" => "string",
                "System.Boolean" => "boolean",
                _ => throw new ArgumentException($"Kusto Type {type} does not map to a known ElasticSearch type"),
            };
        }
    }
}
