// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2Bridge.Models.Response
{
    using System.Collections.Generic;
    using Newtonsoft.Json;

    public class BucketsCollection
    {
        private List<IBucket> buckets = new List<IBucket>();

        [JsonProperty("buckets")]
        public IEnumerable<IBucket> Buckets
        {
            get { return this.buckets; }
        }

        public void AddBucket(IBucket bucket)
        {
            this.buckets.Add(bucket);
        }

        public void AddBuckets(IEnumerable<IBucket> buckets)
        {
            this.buckets.AddRange(buckets);
        }
    }
}
