// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2Bridge.Visitors
{
    using System.Threading.Tasks;
    using K2Bridge.KustoDAL;

    /// <summary>
    /// A helper class for processing types.
    /// </summary>
    public static class ClauseFieldTypeProcessor
    {
        /// <summary>
        /// Get a clause field type.
        /// </summary>
        /// <param name="schemaRetriever">schemaRetriever.</param>
        /// <param name="fieldName">fieldName.</param>
        /// <returns>A ClauseFieldType.</returns>
        public static async Task<ClauseFieldType> GetType(ISchemaRetriever schemaRetriever, string fieldName)
        {
            Ensure.IsNotNull(schemaRetriever, nameof(schemaRetriever), "schemaRetriever cannot be null.");

            Ensure.IsNotNullOrEmpty(fieldName, nameof(fieldName));

            var dic = await schemaRetriever.RetrieveTableSchema();

            // if we failed to get this field type, treat as Unknown
            if (dic.Contains(fieldName) == false)
            {
                return ClauseFieldType.Unknown;
            }

            var fieldType = dic[fieldName];
            return fieldType switch
            {
                "integer" or "long" or "float" or "double" => ClauseFieldType.Numeric,
                "string" or "keyword" => ClauseFieldType.Text,
                "date" => ClauseFieldType.Date,
                _ => ClauseFieldType.Unknown,
            };
        }
    }
}
