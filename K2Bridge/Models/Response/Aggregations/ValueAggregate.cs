// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2Bridge.Models.Response.Aggregations
{
    using Newtonsoft.Json;

    /// <summary>
    /// Describes value aggregate response element.
    /// </summary>
    public class ValueAggregate : IAggregate
    {
        /// <summary>
        /// Gets or sets the value.
        /// </summary>
        [JsonProperty("value")]
        public double? Value { get; set; }
    }
}