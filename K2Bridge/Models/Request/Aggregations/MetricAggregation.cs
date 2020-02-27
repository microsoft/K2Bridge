// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2Bridge.Models.Request.Aggregations
{
    using Newtonsoft.Json;

    /// <summary>
    /// Represents a family of aggregations
    /// that compute metrics based on values extracted from the documents that are being aggregated.
    /// The values are extracted from the fields of the document (using the field data).
    /// </summary>
    internal abstract class MetricAggregation : LeafAggregation
    {
        /// <summary>
        /// Gets or sets field value for metric computation.
        /// </summary>
        [JsonProperty("field")]
        public string FieldName { get; set; }
    }
}
