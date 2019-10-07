namespace K2Bridge
{
    using System;

    public struct QueryData
    {
        public QueryData(string kql, string indexName)
        {
            if (string.IsNullOrEmpty(kql) || string.IsNullOrEmpty(indexName))
            {
                throw new ArgumentException("query string and index name cannot be empty or null");
            }

            this.KQL = kql;
            this.IndexName = indexName;
        }

        public string KQL { get; private set; }

        public string IndexName { get; private set; }
    }
}