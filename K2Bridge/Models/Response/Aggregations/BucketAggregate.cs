// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2Bridge.Models.Response.Aggregations
{
    using System.Collections.Generic;

    /// <summary>
    /// Describes bucket aggregate response element.
    /// </summary>
    public class BucketAggregate<TBucket> : IAggregate
        where TBucket : IBucket
    {
        /// <summary>
        /// Gets or sets a collection of buckets.
        /// </summary>
        public List<TBucket> Buckets { get; set; }
    }
}