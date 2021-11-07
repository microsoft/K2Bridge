// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2Bridge.Models.Response
{
    using K2Bridge.JsonConverters;
    using Newtonsoft.Json;

    /// <summary>
    /// Aggregations response.
    /// </summary>
    [JsonConverter(typeof(ResponseAggregationsConverter))]
    public class Aggregations
    {
        /// <summary>
        /// Gets or sets aggregation value.
        /// </summary>
        public BucketsCollection Collection { get; set; } = new BucketsCollection();

        /// <summary>
        /// Gets or Sets sa.
        /// </summary>
        public string Parent { get; set; }
    }
}
