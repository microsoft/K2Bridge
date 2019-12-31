// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace K2Bridge.Models.Response
{
    using System;
    using System.Collections.Generic;
    using Newtonsoft.Json;

    public class ElasticResponse
    {
        private List<ResponseElement> responses = new List<ResponseElement> { new ResponseElement() };

        [JsonProperty("responses")]
        public IEnumerable<ResponseElement> Responses
        {
            get
            {
                return this.responses;
            }
        }

        public void AddAggregation(IBucket bucket)
        {
            // TODO: support more than one response
            this.responses[0].Aggregations.Collection.AddBucket(bucket);

            // if bucket is DateHistogramBucket, we need to add DocCount to HitsCollection.Total
            if (bucket is DateHistogramBucket)
            {
                this.responses[0].Hits.AddToTotal(bucket.DocCount);
            }
        }

        public IEnumerable<IBucket> GetAllAggregations()
        {
            // TODO: support more than one response
            return this.responses[0].Aggregations.Collection.Buckets;
        }

        public void AddHits(IEnumerable<Hit> hits)
        {
            // TODO: support more than one response
            this.responses[0].Hits.AddHits(hits);
        }

        public IEnumerable<Hit> GetAllHits()
        {
            // TODO: support more than one response
            return this.responses[0].Hits.Hits;
        }

        public void AddTook(TimeSpan timeTaken)
        {
            this.responses[0].TookMilliseconds += timeTaken.Milliseconds;
        }
    }
}
