// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace K2Bridge.Models.Response.Metadata
{
    using System;
    using Newtonsoft.Json;

    [JsonConverter(typeof(FieldCapabilityElementConverter))]
    internal class FieldCapabilityElement
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
            switch (type)
            {
                case "System.Int32":
                    return "integer";
                case "System.Int64":
                    return "long";
                case "System.Single":
                    return "float";
                case "System.Double":
                    return "double";

                case "System.SByte":
                    return "boolean";
                case "System.Object":
                    return "object";
                case "System.String":
                    // Elastic support text and keyword string types. Text is interpreted as something that can't be aggregated, hence we need to choose keyword.
                    return "keyword";
                case "System.DateTime":
                    return "date";
                default:
                    throw new ArgumentException($"Kusto Type {type} does not map to a known ElasticSearch type");
            }
        }
    }
}
