// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2Bridge.Models.Response.Metadata
{
    using System.Collections.Generic;
    using Newtonsoft.Json;

    /// <summary>
    /// Field capability response.
    /// </summary>
    public class FieldCapabilityResponse
    {
        private readonly Dictionary<string, FieldCapabilityElement> fields = new Dictionary<string, FieldCapabilityElement>();
        private readonly List<string> indices = new List<string>();

        /// <summary>
        /// Gets all fields.
        /// </summary>
        /// <returns>Dictionary of all fields with key field name and field capability element value.</returns>
        [JsonProperty("fields")]
        public IDictionary<string, FieldCapabilityElement> Fields => fields;

        /// <summary>
        /// Indices.
        /// </summary>
        /// <returns>List of indices.</returns>
        [JsonProperty("indices")]
        public IEnumerable<string> Indices {
            get { return indices; }
        }

        /// <summary>
        /// Add field capability element to response.
        /// </summary>
        /// <param name="fieldCapabilityElement">Added field capability element.</param>
        public void AddField(FieldCapabilityElement fieldCapabilityElement)
        {
            Ensure.IsNotNull(fieldCapabilityElement, nameof(fieldCapabilityElement));
            Ensure.IsNotNull(fieldCapabilityElement.Name, nameof(fieldCapabilityElement.Name));

            fields.Add(fieldCapabilityElement.Name, fieldCapabilityElement);
        }

        /// <summary>
        /// Add index to response.
        /// </summary>
        /// <param name="fieldCapabilityElement">Index name.</param>
        public void AddIndex(string indexName)
        {
            indices.Add(indexName);
        }
    }
}
