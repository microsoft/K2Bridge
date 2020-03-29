// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2Bridge.KustoDAL
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Data.SqlTypes;
    using System.Linq;
    using System.Xml;
    using K2Bridge.Factories;
    using K2Bridge.Models;
    using K2Bridge.Models.Response;
    using K2Bridge.Utils;

    /// <summary>
    /// Provides parsing for hit rows in Data Explorer response objects.
    /// </summary>
    public static class HitsMapper
    {
        /// <summary>
        /// Type converter function used to get values from column.
        /// </summary>
        private static readonly Dictionary<Type, Func<object, object>> Converters = new Dictionary<Type, Func<object, object>>
        {
            { typeof(sbyte), (value) => value is DBNull || value == null ? null : (bool?)((sbyte)value != 0) },
            { typeof(SqlDecimal), (value) => value.Equals(SqlDecimal.Null) ? double.NaN : ((SqlDecimal)value).ToDouble() },
            { typeof(Guid), (value) => value is DBNull || value == null ? null : ((Guid)value).ToString() },
            { typeof(TimeSpan), (value) => value is DBNull || value == null ? null : XmlConvert.ToString((TimeSpan)value) },

            // Elasticsearch returns timestamp fields in UTC in ISO-8601 but without Timezone.
            // Use a String type to control serialization to mimic this behavior.
            { typeof(DateTime), (value) => value is DBNull ? null : ((DateTime)value).ToString("yyyy-MM-dd'T'HH:mm:ss.FFFFFFF") },
        };

        private static readonly Random Random = new Random();

        /// <summary>
        /// Parses a kusto datatable to hits.
        /// </summary>
        /// <param name="kustoResponseDataTable">Kusto data rows.</param>
        /// <param name="query">QueryData containing query information.</param>
        /// <param name="highlighter">Lucene highlighter.</param>
        /// <returns>Hits IEnumerable.</returns>
        internal static IEnumerable<Hit> MapRowsToHits(DataRowCollection kustoResponseDataTable, QueryData query, LuceneHighlighter highlighter)
        {
            return MapAndAnalyzeRows(kustoResponseDataTable, query, highlighter);
        }

        private static IEnumerable<Hit> MapAndAnalyzeRows(DataRowCollection kustoResponseDataTable, QueryData query, LuceneHighlighter highlighter)
            => kustoResponseDataTable.OfType<DataRow>().Select(row => ReadHit(row, query, highlighter));

        /// <summary>
        /// Get a Hit model from a kusto row.
        /// </summary>
        /// <param name="row">Kusto data row.</param>
        /// <param name="query">QueryData containing query information.</param>
        /// <param name="highlighter">Lucene Highligher.</param>
        /// <returns>Hit model.</returns>
        private static Hit ReadHit(DataRow row, QueryData query, LuceneHighlighter highlighter)
        {
            Ensure.IsNotNull(row, nameof(row));

            var hit = HitsFactory.Create(Random.Next().ToString(), query.IndexName);
            var columns = row.Table.Columns;

            for (int columnIndex = 0; columnIndex < row.ItemArray.Length; columnIndex++)
            {
                var columnName = columns[columnIndex].ColumnName;
                var columnValue = GetTypedValueFromColumn(columns[columnIndex], row[columnName]);
                hit.AddSource(columnName, columnValue);
                var highlightValue = highlighter.GetHighlightedValue(columnName, columnValue);
                if (!string.IsNullOrEmpty(highlightValue))
                {
                    hit.AddColumnHighlight(columnName, new List<string> { highlightValue });
                }
            }

            CreateSort(hit, row, query);
            CreateField(hit, row, query);

            return hit;
        }

        private static void CreateSort(Hit hit, DataRow row, QueryData query)
        {
            if (query.SortFields == null)
            {
                return;
            }

            foreach (var sortField in query.SortFields)
            {
                var value = row.Table.Columns.Contains(sortField) ? row[sortField] : null;
                if (value == null)
                {
                    continue;
                }

                if (value is DateTime)
                {
                    value = TimeUtils.ToEpochMilliseconds((DateTime)value);
                }

                hit.Sort.Add(value);
            }
        }

        private static void CreateField(Hit hit, DataRow row, QueryData query)
        {
            if (query.DocValueFields == null)
            {
                return;
            }

            foreach (var docValueField in query.DocValueFields)
            {
                var value = row.Table.Columns.Contains(docValueField) ? row[docValueField] : null;
                if (value == null)
                {
                    continue;
                }

                // datetime fields are written as is, usually in UTC timezone.
                hit.Fields.Add(docValueField, new List<object> { value });
            }
        }

        /// <summary>
        /// Get a typed object from a Datacolumn.
        /// </summary>
        /// <param name="column">DataColumn to get type from.</param>
        /// <param name="value">Original value.</param>
        /// <returns>The converted type value.</returns>
        private static object GetTypedValueFromColumn(DataColumn column, object value)
        {
            if (value == null)
            {
                return null;
            }

            var type = column.DataType;
            if (Converters.ContainsKey(type))
            {
                return Converters[type](value);
            }

            return value;
        }
    }
}