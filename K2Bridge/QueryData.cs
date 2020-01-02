// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2Bridge
{
    using System;
    using System.Collections.Generic;

    public struct QueryData
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="QueryData"/> struct.
        /// </summary>
        /// <param name="kql"></param>
        /// <param name="indexName"></param>
        /// <param name="highlightText"></param>
        public QueryData(string kql, string indexName, Dictionary<string, string> highlightText)
        {
            if (string.IsNullOrEmpty(kql) || string.IsNullOrEmpty(indexName))
            {
                throw new ArgumentException("query string and index name cannot be empty or null");
            }

            this.KQL = kql;
            this.IndexName = indexName;
            this.HighlightText = highlightText;
            this.HighlightPreTag = string.Empty;
            this.HighlightPostTag = string.Empty;
        }

        public string KQL { get; private set; }

        public string IndexName { get; private set; }

        public Dictionary<string, string> HighlightText { get; private set; }

        public string HighlightPreTag { get; set; }

        public string HighlightPostTag { get; set; }
    }
}