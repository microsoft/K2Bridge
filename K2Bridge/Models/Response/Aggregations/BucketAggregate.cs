// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2Bridge.Models.Response.Aggregations
{
    using System.Collections.Generic;
    using K2Bridge.JsonConverters;
    using Newtonsoft.Json;

    /// <summary>
    /// Describes bucket aggregate response element.
    /// </summary>
    [JsonConverter(typeof(BucketAggregateConverter))]
    public class BucketAggregate<TBucket> : IAggregate
        where TBucket : IBucket
    {
        /// <summary>
        /// Gets or sets a collection of buckets.
        /// </summary>
        public List<TBucket> Buckets { get; } = new List<TBucket>();

        /// <summary>
        /// Gets or sets the Keyed value.
        /// </summary>
        public bool Keyed { get; set; } = false;
    }
}