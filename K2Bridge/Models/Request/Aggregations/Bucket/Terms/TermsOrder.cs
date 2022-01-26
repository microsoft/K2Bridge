// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2Bridge.Models.Request.Aggregations
{
    internal class TermsOrder
    {
        /// <summary>
        /// Gets or sets the field name used to sort buckets.
        /// </summary>
        public string SortField { get; set; } = "count_";

        /// <summary>
        /// Gets or sets the ordering of the results.
        /// </summary>
        public string SortOrder { get; set; } = "desc";
    }
}
