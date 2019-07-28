﻿namespace K2Bridge
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Text;
    using System.Data;
    using K2Bridge.KustoConnector;
    using Newtonsoft.Json;
    using System.Security.Cryptography;
    using System.Collections.Generic;

    internal class KibanaRequestHandler : RequestHandler
    {
        public KibanaRequestHandler(HttpListenerContext requestContext, KustoManager kustoClient, Guid requestId)
            :base(requestContext, kustoClient, requestId)
        {
        }

        public Guid GetIndexGuid(string tableName)
        {
            if (tableName == "kibana_sample_data_flights")
            { return new Guid("d3d7af60-4c81-11e8-b3d7-01146121b73d"); }
            else if (tableName == "kibana_sample_data_ecommerce")
            { return new Guid("ff959d40-b880-11e8-a6d9-e546fe2bba5f"); }
            else if (tableName == "kibana_sample_data_logs")
            { return new Guid("90943e30-9a47-11e8-b64d-95841ca0b247"); }
            else
            {
                return StringToGUID(tableName);
            }
        }

        protected List<KustoConnector.Hit> PrepareHits(string indexPatternId = null)
        {
            IDataReader kustoResults = this.kusto.ExecuteControlCommand(".show database schema");

            List<KustoConnector.Hit> hitsList = new List<KustoConnector.Hit>();

            KustoConnector.Hit hit = null;
            StringBuilder sbFields = null;

            while (kustoResults.Read())
            {
                IDataRecord record = (IDataRecord)kustoResults;

                string tableName = record[1].ToString();
                string fieldName = record[2].ToString();
                string fieldType = record[3].ToString();
                string indexID = GetIndexGuid(tableName).ToString();

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

                        if (indexPatternId == hit._id)
                        {
                            hitsList.Add(hit);
                        }
                    }

                    // Starting a new table 
                    sbFields = new StringBuilder("[");
                    hit = new KustoConnector.Hit();
                    hit._source = new KustoConnector.Source();
                    hit._source.index_pattern = new KustoConnector.IndexPattern();


                    hit._index = ".kibana_1";
                    hit._type = "doc";
                    hit._id = $"index-pattern:{indexID}";
                    hit._seq_no = 55;
                    hit._primary_term = 1;
                    hit._score = 0.0;
                    hit._source.type = "index-pattern";
                    hit._source.migrationVersion = new MigrationVersion();
                    hit._source.migrationVersion.index_pattern = "6.5.0";
                    hit._source.updated_at = "2019-07-16T16:19:23.016Z"; //TODO what value shoudl go in here?
                    hit._source.index_pattern.title = tableName;
                    hit._source.index_pattern.fieldFormatMap = "{\"hour_of_day\":{}}"; //TODO this is not clear why it is done this way
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
