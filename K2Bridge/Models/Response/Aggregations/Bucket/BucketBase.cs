// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2Bridge.Models.Response.Aggregations
{
    using Newtonsoft.Json;

    /// <summary>
    /// Describes abstract base class for bucket response element.
    /// </summary>
    public abstract class BucketBase : AggregateDictionary, IBucket
    {
        /// <summary>
        /// Gets or sets the doc_count value.
        /// </summary>
        [JsonProperty("doc_count")]
        public long DocCount { get; set; }
    }
}