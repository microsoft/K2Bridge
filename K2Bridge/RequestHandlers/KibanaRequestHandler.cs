namespace K2Bridge
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Net;
    using System.Security.Cryptography;
    using System.Text;
    using K2Bridge.KustoConnector;
    using Microsoft.Extensions.Logging;

    internal class KibanaRequestHandler : RequestHandler
    {
        public KibanaRequestHandler(HttpListenerContext requestContext, IQueryExecutor kustoClient, string requestId, ILogger logger)
            : base(requestContext, kustoClient, requestId, logger)
        {
        }

        public string GetIndexGuid(string tableName)
        {
            Guid g;
            if (tableName == "kibana_sample_data_flights")
            {
                g = new Guid("d3d7af60-4c81-11e8-b3d7-01146121b73d");
            }
            else if (tableName == "kibana_sample_data_ecommerce")
            {
                g = new Guid("ff959d40-b880-11e8-a6d9-e546fe2bba5f");
            }
            else if (tableName == "kibana_sample_data_logs")
            {
                g = new Guid("90943e30-9a47-11e8-b64d-95841ca0b247");
            }
            else
            {
                g = StringToGUID(tableName);
            }

            return g.ToString();
        }

        protected List<Models.Metadata.Hit> PrepareHits(string indexPatternId = null)
        {
            IDataReader kustoResults = this.kusto.ExecuteControlCommand(".show database schema");

            List<Models.Metadata.Hit> hitsList = new List<Models.Metadata.Hit>();

            Models.Metadata.Hit hit = null;
            StringBuilder sbFields = null;

            while (kustoResults.Read())
            {
                IDataRecord record = (IDataRecord)kustoResults;

                string tableName = record[1].ToString();
                string fieldName = record[2].ToString();
                string fieldType = record[3].ToString();
                string indexID = GetIndexGuid(tableName);

                if (tableName == string.Empty)
                {
                    // First line of results is DB name only.
                    continue;
                }

                if (fieldName == null || fieldName == string.Empty)
                {
                    if (hit != null)
                    {
                        // Wrap the previous table
                        sbFields.Append("]");

                        hit._source.index_pattern.fields = sbFields.ToString();

                        if (indexPatternId == null || indexPatternId == hit._id)
                        {
                            hitsList.Add(hit);
                        }
                    }

                    // Starting a new table
                    sbFields = new StringBuilder("[");
                    hit = new Models.Metadata.Hit();
                    hit._source = new Models.Metadata.Source();
                    hit._source.index_pattern = new Models.Metadata.IndexPattern();

                    hit._index = ".kibana_1";
                    hit._type = "doc";
                    hit._id = $"index-pattern:{indexID}";
                    hit._seq_no = 55;
                    hit._primary_term = 1;
                    hit._score = 0.0;
                    hit._source.type = "index-pattern";
                    hit._source.migrationVersion = new Models.Metadata.MigrationVersion();
                    hit._source.migrationVersion.index_pattern = "6.5.0";
                    hit._source.updated_at = "2019-07-16T16:19:23.016Z"; //TODO what value shoudl go in here?
                    hit._source.index_pattern.title = tableName;
                    hit._source.index_pattern.fieldFormatMap = "{\"hour_of_day\":{}}"; //TODO this is not clear why it is done this way

                    //
                    // Hack
                    if (indexID == "d3d7af60-4c81-11e8-b3d7-01146121b73d")
                    {
                        hit._source.index_pattern.timeFieldName = "timestamp";
                        hit._source.index_pattern.fieldFormatMap = "{\"hour_of_day\":{\"id\":\"number\",\"params\":{\"pattern\":\"00\"}},\"AvgTicketPrice\":{\"id\":\"number\",\"params\":{\"pattern\":\"$0,0.[00]\"}}}";
                    }
                }
                else
                {
                    // Adding a field
                    if (hit._source.index_pattern.timeFieldName == null && fieldType == "System.DateTime")
                    {
                        hit._source.index_pattern.timeFieldName = fieldName;
                    }

                    if (sbFields.ToString() != "[")
                    {
                        sbFields.Append(",");
                    }

                    sbFields.Append("{");
                    AddAttributeToStringBuilder(sbFields, "name", fieldName);
                    sbFields.Append(",");
                    AddAttributeToStringBuilder(sbFields, "type", ElasticTypeFromKustoType(fieldType));
                    sbFields.Append(",");
                    AddAttributeToStringBuilder(sbFields, "count", 0);
                    sbFields.Append(",");
                    AddAttributeToStringBuilder(sbFields, "scripted", false);
                    sbFields.Append(",");
                    AddAttributeToStringBuilder(sbFields, "searchable", true);
                    sbFields.Append(",");
                    AddAttributeToStringBuilder(sbFields, "aggregatable", true);
                    sbFields.Append(",");
                    AddAttributeToStringBuilder(sbFields, "readFromDocValues", false);
                    sbFields.Append("}");
                }
            }

            // Wrap the previous table
            sbFields.Append("]");
            hit._source.index_pattern.fields = sbFields.ToString();

            if (indexPatternId == null || indexPatternId == string.Empty || indexPatternId == hit._id)
            {
                hitsList.Add(hit);
            }

            kustoResults.Close();

            return hitsList;
        }

        public static Guid StringToGUID(string value)
        {
            // Create a new instance of the MD5CryptoServiceProvider object.
            MD5 md5Hasher = MD5.Create();
            // Convert the input string to a byte array and compute the hash.
            byte[] data = md5Hasher.ComputeHash(Encoding.Default.GetBytes(value));
            return new Guid(data);
        }

        protected void AddAttributeToStringBuilder(StringBuilder sb, string attributeName, string attributeValue)
        {
            sb.Append("\"");
            sb.Append(attributeName);
            sb.Append("\":\"");
            sb.Append(attributeValue);
            sb.Append("\"");
        }

        protected void AddAttributeToStringBuilder(StringBuilder sb, string attributeName, int attributeValue)
        {
            sb.Append("\"");
            sb.Append(attributeName);
            sb.Append("\":");
            sb.Append(attributeValue.ToString());
        }

        protected void AddAttributeToStringBuilder(StringBuilder sb, string attributeName, bool attributeValue)
        {
            sb.Append("\"");
            sb.Append(attributeName);
            sb.Append("\":");
            sb.Append(attributeValue.ToString().ToLower());
        }

        protected string ElasticTypeFromKustoType(string type)
        {
            if ("System.DateTime" == type)
                return "date";
            else if ("System.Int32" == type)
                return "number";
            else if ("System.Int64" == type)
                return "number";
            else if ("System.Double" == type)
                return "number";
            else if ("System.Single" == type)
                return "number";
            else if ("System.SByte" == type)
                return "bool";
            else if ("System.Object" == type)
                return "json";

            return null;
        }
    }
}
