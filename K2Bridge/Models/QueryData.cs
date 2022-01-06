// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2Bridge.Models
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Represents the data needed to search for information in the database.
    /// </summary>
    public struct QueryData
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="QueryData"/> struct.
        /// </summary>
        /// <param name="queryCommandText">The query to execute.</param>
        /// <param name="indexName">The index to be searched.</param>
        /// <param name="sortFields">Field names specified in query sort clause.</param>
        /// <param name="docValueFields">Field names specified in query docvalue_fields clause.</param>
        /// <param name="highlightText">What terms need to be highlighted in the results.</param>
        public QueryData(
            string queryCommandText,
            string indexName,
            IList<string> sortFields = null,
            IList<string> docValueFields = null,
            Dictionary<string, string> highlightText = null)
        {
            Ensure.IsNotNullOrEmpty(queryCommandText, nameof(queryCommandText), "Query string cannot be empty or null");
            Ensure.IsNotNullOrEmpty(indexName, nameof(indexName), "Index name string cannot be empty or null");

            QueryCommandText = queryCommandText;
            IndexName = indexName;
            SortFields = sortFields;
            DocValueFields = docValueFields;
            HighlightText = highlightText;
            HighlightPreTag = string.Empty;
            HighlightPostTag = string.Empty;
            PrimaryAggregation = default;
        }

        /// <summary>
        /// Gets the query to execute.
        /// </summary>
        public string QueryCommandText { get; private set; }

        /// <summary>
        /// Gets the index name to be searched.
        /// </summary>
        public string IndexName { get; private set; }

        /// <summary>
        /// Gets the field names to sort by.
        /// </summary>
        public IList<string> SortFields { get; private set; }

        /// <summary>
        /// Gets the field names to be returned in the fields element.
        /// </summary>
        public IList<string> DocValueFields { get; private set; }

        /// <summary>
        /// Gets the Highlight request info.
        /// </summary>
        public Dictionary<string, string> HighlightText { get; private set; }

        /// <summary>
        /// Gets or sets the tag that opens highlighted text.
        /// </summary>
        public string HighlightPreTag { get; internal set; }

        /// <summary>
        /// Gets or sets the tag that closes highlighted text.
        /// </summary>
        public string HighlightPostTag { get; internal set; }

        /// <summary>
        /// Gets the primary aggregation.
        /// </summary>
        public KeyValuePair<string, string> PrimaryAggregation { get; internal set; }
    }
}
