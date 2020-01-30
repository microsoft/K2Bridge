// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2Bridge.Models
{
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
        /// <param name="highlightText">What terms need to be highlighted in the results.</param>
        public QueryData(string queryCommandText, string indexName, Dictionary<string, string> highlightText)
        {
            Ensure.IsNotNullOrEmpty(queryCommandText, nameof(queryCommandText), "Query string cannot be empty or null");
            Ensure.IsNotNullOrEmpty(indexName, nameof(indexName), "Index name string cannot be empty or null");

            QueryCommandText = queryCommandText;
            IndexName = indexName;
            HighlightText = highlightText;
            HighlightPreTag = string.Empty;
            HighlightPostTag = string.Empty;
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
        /// Gets the Highlight request info.
        /// </summary>
        public Dictionary<string, string> HighlightText { get; private set; }

        /// <summary>
        /// Gets or sets the tag that opens highlighted text.
        /// </summary>
        public string HighlightPreTag { get; set; }

        /// <summary>
        /// Gets or sets the tag that closes highlighted text.
        /// </summary>
        public string HighlightPostTag { get; set; }
    }
}