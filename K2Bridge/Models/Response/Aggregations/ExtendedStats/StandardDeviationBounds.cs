// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2Bridge.Models.Response.Aggregations
{
    using Newtonsoft.Json;

    /// <summary>
    /// Describes Standard Deviation bounds response element.
    /// </summary>
    public class StandardDeviationBounds
    {
        /// <summary>
        /// Gets or sets the upper value for the StdDev upper bound.
        /// </summary>
        [JsonProperty("upper")]
        public double? Upper { get; set; }

        /// <summary>
        /// Gets or sets the lower value for the StdDev lower bound.
        /// </summary>
        [JsonProperty("lower")]
        public double? Lower { get; set; }
    }
}