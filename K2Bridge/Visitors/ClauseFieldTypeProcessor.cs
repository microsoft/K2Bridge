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
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Reliability", "CA2007:Consider calling ConfigureAwait on the awaited task", Justification = "await it valid.")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Globalization", "CA1303:Do not pass literals as localized parameters", Justification = "Resources are not supported yet.")]
        public static async Task<ClauseFieldType> GetType(ISchemaRetriever schemaRetriever, string fieldName)
        {
            if (schemaRetriever == null)
            {
                throw new System.Exception("schemaRetriever cannot be null.");
            }

            Ensure.IsNotNullOrEmpty(fieldName, nameof(fieldName));

            var dic = await schemaRetriever.RetrieveTableSchema();

            // if we failed to get this field type, treat as Unknown
            if (dic.Contains(fieldName) == false)
            {
                return ClauseFieldType.Unknown;
            }

            var fieldType = dic[fieldName];
            switch (fieldType)
            {
                case "integer":
                case "long":
                case "float":
                case "double":
                    return ClauseFieldType.Numeric;
                case "string":
                    return ClauseFieldType.Text;
                case "date":
                    return ClauseFieldType.Date;
                default:
                    return ClauseFieldType.Unknown;
            }
        }
    }
}
