// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2Bridge.Models.Response.Metadata
{
    using System;
    using Newtonsoft.Json;

    [JsonConverter(typeof(FieldCapabilityElementConverter))]
    public class FieldCapabilityElement
    {
        private enum DataReaderMapping
        {
            ColumnName = 0,
            ColumnType = 1,
        }

        public string Name { get; set; }

        public string Type { get; set; }

        public bool IsAggregatable { get; set; } = true;

        public bool IsSearchable { get; set; } = true;

        public static FieldCapabilityElement Create(System.Data.IDataRecord record)
        {
            Ensure.IsNotNull(record, nameof(record));

            var columnName = record[(int)DataReaderMapping.ColumnName];
            var columnType = record[(int)DataReaderMapping.ColumnType];

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
