// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2Bridge.Models.Response
{
    using System;
    using System.Collections.Generic;
    using Newtonsoft.Json;

    /// <summary>
    /// Elastic response as expected by Kibana.
    /// </summary>
    public class ElasticResponse
    {
        private readonly List<ResponseElement> responses = new () { new ResponseElement() };

        /// <summary>
        /// Gets responses.
        /// In our case response will always contain just one response.
        /// </summary>
        [JsonProperty("responses")]
        public IEnumerable<ResponseElement> Responses => responses;

        /// <summary>
        /// Gets or sets query execution time.
        /// </summary>
        [JsonProperty("took")]
        public int TookMilliseconds { get; set; }

        /// <summary>
        /// Add parent to the main agg
        /// </summary>
        public void AddParentToAgg(string parent)
        {
            responses[0].Aggregations.Parent = parent;
        }

        /// <summary>
        /// Add aggregation to first response.
        /// </summary>
        /// <param name="bucket">The added bucket.</param>
        public void AddBucketToAggregation(IBucket bucket)
        {
            // TODO: support more than one response
            responses[0].Aggregations.Collection.AddBucket(bucket);

            // Add DocCount to HitsCollection.Total
            responses[0].Hits.AddToTotal(bucket.DocCount);
        }

        /// <summary>
        /// Set aggregation keyed flag on first response.
        /// </summary>
        /// <param name="isKeyed">The added bucket.</param>
        public void SetAggregationKeyed(bool keyed)
        {
            // TODO: support more than one response
            responses[0].Aggregations.Collection.Keyed = keyed;
        }

        /// <summary>
        /// Retrieve all aggregations.
        /// </summary>
        /// <returns>IEnumerable of all aggregations.</returns>
        public IEnumerable<IBucket> GetAllAggregations()
        {
            // TODO: support more than one response
            return responses[0].Aggregations.Collection.Buckets;
        }

        /// <summary>
        /// Add hits to first response.
        /// </summary>
        /// <param name="hits">The added hits.</param>
        public void AddHits(IEnumerable<Hit> hits)
        {
            // TODO: support more than one response
            responses[0].Hits.AddHits(hits);
        }

        /// <summary>
        /// Retrieve all hits.
        /// </summary>
        /// <returns>IEnumerable of all hits in the responses.</returns>
        public IEnumerable<Hit> GetAllHits()
        {
            // TODO: support more than one response
            return responses[0].Hits.Hits;
        }

        /// <summary>
        /// Add the query execution time.
        /// </summary>
        /// <param name="timeTaken">Time to add.</param>
        public void AddTook(TimeSpan timeTaken)
        {
            responses[0].TookMilliseconds += timeTaken.Milliseconds;
            TookMilliseconds += timeTaken.Milliseconds;
        }

        /// <summary>
        /// Adds to HitsTotal of the first response.
        /// </summary>
        /// <param name="count">An int to increment by.</param>
        public void AddToTotal(int count)
        {
            responses[0].Hits.AddToTotal(count);
        }

        /// <summary>
        /// Add the translated and executed ADX query.
        /// </summary>
        /// <param name="query">Query string.</param>
        internal void AppendBackendQuery(string query)
        {
            responses[0].BackendQuery = query;
        }
    }
}
