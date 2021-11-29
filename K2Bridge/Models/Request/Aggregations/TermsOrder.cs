// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2Bridge.Models.Request.Aggregations
{
    using K2Bridge.JsonConverters;
    using K2Bridge.Visitors;
    using Newtonsoft.Json;

    /// <summary>
    /// Ordering of buckets for TermsAggregation.
    /// </summary>
    [JsonConverter(typeof(TermsOrderConverter))]
    internal class TermsOrder
    {
        /// <summary>
        /// Gets or sets the sort key.
        /// </summary>
        public string Key { get; set; } = "_count";

        /// <summary>
        /// Gets or sets the sort order.
        /// </summary>
        public string Order { get; set; } = "desc";
    }
}
