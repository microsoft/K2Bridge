// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2Bridge.Models.Response
{
    using System.Collections.Generic;
    using K2Bridge.JsonConverters;
    using Newtonsoft.Json;

    /// <summary>
    /// Buckets response.
    /// </summary>
    [JsonConverter(typeof(BucketsCollectionConverter))]
    public class BucketsCollection
    {
        private readonly List<Bucket> buckets = new ();

        /// <summary>
        /// Gets all buckets.
        /// </summary>
        [JsonProperty("buckets")]
        public IEnumerable<Bucket> Buckets
        {
            get { return buckets; }
        }

        /// <summary>
        /// Gets or sets the keyed flag.
        /// </summary>
        public bool Keyed { get; set; } = false;

        /// <summary>
        /// Add bucket to buckets response.
        /// </summary>
        /// <param name="bucket">Added bucket object.</param>
        public void AddBucket(Bucket bucket)
        {
            buckets.Add(bucket);
        }

        /// <summary>
        /// Add multiple buckets to buckets response.
        /// </summary>
        /// <param name="buckets">IEnumerable of bucket objects for addition to response.</param>
        public void AddBuckets(IEnumerable<Bucket> buckets)
        {
            this.buckets.AddRange(buckets);
        }
    }
}
