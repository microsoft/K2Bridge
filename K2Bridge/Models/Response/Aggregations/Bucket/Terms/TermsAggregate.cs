// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2Bridge.Models.Response.Aggregations
{
    /// <summary>
    /// Describes terms aggregate response element.
    /// </summary>
    public class TermsAggregate : BucketAggregate
    {
        /// <summary>
        /// Gets or sets the DocCountErrorUpperBound value.
        /// </summary>
        public long? DocCountErrorUpperBound { get; set; }

        /// <summary>
        /// Gets or sets the SumOtherDocCount value.
        /// </summary>
        public long? SumOtherDocCount { get; set; }
    }
}
