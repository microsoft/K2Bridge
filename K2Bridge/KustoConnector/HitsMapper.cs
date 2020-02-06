// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2Bridge.KustoConnector
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using K2Bridge.Models;
    using K2Bridge.Models.Response;

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
            { typeof(sbyte), (value) => (sbyte)value != 0 },

            // Elasticsearch returns timestamp fields in UTC in ISO-8601 but without Timezone.
            // Use a String type to control serialization to mimic this behavior.
            { typeof(DateTime), (value) => value != null ? ((DateTime)value).ToString("yyyy-MM-dd'T'HH:mm:ss.FFFFFFF") : null },
        };

        private static readonly Random Random = new Random();

        /// <summary>
        /// Parses a kusto datatable to hits.
        /// </summary>
        /// <param name="kustoResponseDataTable">Kusto data row.</param>
        /// <param name="query">QueryData containing query information.</param>
        /// <returns>Hits IEnumerable.</returns>
        internal static IEnumerable<Hit> MapDataTableToHits(DataRowCollection kustoResponseDataTable, QueryData query)
        {
            // Skipping highlight if the query's HighlightText dictionary is empty or if pre/post tags are empty.
            var isHighlight = query.HighlightText != null && query.HighlightPreTag != null && query.HighlightPostTag != null;
            foreach (DataRow row in kustoResponseDataTable)
            {
                yield return ReadHit(row, query, isHighlight);
            }
        }

        /// <summary>
        /// Get a Hit model from a kusto row.
        /// </summary>
        /// <param name="row">Kusto data row.</param>
        /// <param name="query">QueryData containing query information.</param>
        /// <param name="highlight">highlight the hit.</param>
        /// <returns>Hit model.</returns>
        private static Hit ReadHit(DataRow row, QueryData query, bool highlight)
        {
            Ensure.IsNotNull(row, nameof(row));

            var hit = Hit.Create(Random.Next().ToString(), query.IndexName);
            var columns = row.Table.Columns;

            for (int index = 0; index < row.ItemArray.Length; index++)
            {
                var name = columns[index].ColumnName;
                var value = GetTypedValueFromColumn(columns[index], row[name]);
                hit.AddSource(name, value);

                if (highlight && value != null)
                {
                    // Elastic only highlights string values, but we try to highlight everything we can here.
                    // To mimic elastic: check for type of value here and skip if != string.
                    HighlightHit(hit, query, name, value.ToString());
                }
            }

            CreateSort(hit, row, query);
            hit.Fields = new Fields();
            return hit;
        }

        /// <summary>
        /// Add highlight metadata to hit.
        /// </summary>
        /// <param name="hit">Hit object.</param>
        /// <param name="query">QueryData containing query information.</param>
        /// <param name="name">Name of field to add hit.</param>
        /// <param name="stringValue">value of the field as string.</param>
        private static void HighlightHit(Hit hit, QueryData query, string name, string stringValue)
        {
            // HighlightText.ContainsKey(name) condition will be true when searching with the available filters
            // HighlightText.ContainsKey("*") condition will be true when searching with the search box
            if (query.HighlightText.ContainsKey(name) && stringValue.Equals(query.HighlightText[name], StringComparison.OrdinalIgnoreCase))
            {
                hit.AddColumnHighlight(name, new List<string> { query.HighlightPreTag + query.HighlightText[name] + query.HighlightPostTag });
            }
            else if (query.HighlightText.ContainsKey("*"))
            {
                hit.AddColumnHighlight(name, new List<string> { query.HighlightPreTag + query.HighlightText["*"] + query.HighlightPostTag });
            }
        }

        private static void CreateSort(Hit hit, DataRow row, QueryData query)
        {
            if (query.SortFields == null)
            {
                return;
            }

            foreach (var sortField in query.SortFields)
            {
                object value;
                try
                {
                    value = row[sortField];
                }
                catch (ArgumentException)
                {
                    // Sorting by a column not in the hit list. Retrieving value is not supported.
                    continue;
                }

                if (value is DateTime)
                {
                    value = TimeUtils.ToEpochMilliseconds((DateTime)value);
                }

                hit.Sort.Add(value);
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