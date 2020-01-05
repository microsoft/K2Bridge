// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2Bridge.Models.Response
{
    using System;
    using System.Collections.Generic;
    using Newtonsoft.Json;

    public class ElasticResponse
    {
        private readonly List<ResponseElement> responses = new List<ResponseElement> { new ResponseElement() };

        [JsonProperty("responses")]
        public IEnumerable<ResponseElement> Responses => responses;

        public void AddAggregation(IBucket bucket)
        {
            // TODO: support more than one response
            responses[0].Aggregations.Collection.AddBucket(bucket);

            // if bucket is DateHistogramBucket, we need to add DocCount to HitsCollection.Total
            if (bucket is DateHistogramBucket)
            {
                responses[0].Hits.AddToTotal(bucket.DocCount);
            }
        }

        public IEnumerable<IBucket> GetAllAggregations()
        {
            // TODO: support more than one response
            return responses[0].Aggregations.Collection.Buckets;
        }

        public void AddHits(IEnumerable<Hit> hits)
        {
            // TODO: support more than one response
            responses[0].Hits.AddHits(hits);
        }

        public IEnumerable<Hit> GetAllHits()
        {
            // TODO: support more than one response
            return responses[0].Hits.Hits;
        }

        public void AddTook(TimeSpan timeTaken)
        {
            responses[0].TookMilliseconds += timeTaken.Milliseconds;
        }

        internal void AppendBackendQuery(string query)
        {
            responses[0].BackendQuery = query;
        }
    }
}
