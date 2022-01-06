// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2Bridge.Models.Response.Aggregations
{
    using Newtonsoft.Json;

    /// <summary>
    /// Describes extended stats response element for standard deviation.
    /// </summary>
    public class ExtendedStatsAggregate : IAggregate
    {
        /// <summary>
        /// Gets or sets the average value for the extended stats aggregate.
        /// </summary>
        [JsonProperty("avg")]
        public double? Average { get; set; }

        /// <summary>
        /// Gets or sets the standard deviation value for the extended stats aggregate.
        /// </summary>
        [JsonProperty("std_deviation")]
        public double? StandardDeviation { get; set; }

        /// <summary>
        /// Gets or sets the standard deviation bounds valuse for the extended stats aggregate.
        /// </summary>
        [JsonProperty("std_deviation_bounds")]
        public StandardDeviationBounds StandardDeviationBounds { get; set; } = new StandardDeviationBounds();
    }
}