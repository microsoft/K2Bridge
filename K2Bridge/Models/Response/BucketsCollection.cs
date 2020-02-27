// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2Bridge.Models.Response
{
    using System.Collections.Generic;
    using Newtonsoft.Json;

    /// <summary>
    /// Buckets response.
    /// </summary>
    public class BucketsCollection
    {
        private readonly List<IBucket> buckets = new List<IBucket>();

        /// <summary>
        /// Gets all buckets.
        /// </summary>
        [JsonProperty("buckets")]
        public IEnumerable<IBucket> Buckets
        {
            get { return buckets; }
        }

        /// <summary>
        /// Add bucket to buckets response.
        /// </summary>
        /// <param name="bucket">Added bucket object.</param>
        public void AddBucket(IBucket bucket)
        {
            buckets.Add(bucket);
        }

        /// <summary>
        /// Add multiple buckets to buckets response.
        /// </summary>
        /// <param name="buckets">IEnumerable of bucket objects for addition to response.</param>
        public void AddBuckets(IEnumerable<IBucket> buckets)
        {
            this.buckets.AddRange(buckets);
        }
    }
}
