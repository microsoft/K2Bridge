// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2Bridge.Models.Response.Aggregations
{
    /// <summary>
    /// Describes percentile item response element.
    /// </summary>
    public class PercentileItem
    {
        /// <summary>
        /// Gets or sets the percentile.
        /// </summary>
        public double Percentile { get; set; }

        /// <summary>
        /// Gets or sets the percentile Value.
        /// </summary>
        public double? Value { get; set; }

        /// <summary>
        /// Gets or sets the percentile ValueAsString.
        /// </summary>
        public string ValueAsString { get; set; }
    }
}
