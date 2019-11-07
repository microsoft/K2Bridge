namespace K2Bridge
{
    using System;
    using System.Collections.Generic;

    public struct QueryData
    {
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