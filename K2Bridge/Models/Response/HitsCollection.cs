// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2Bridge.Models.Response
{
    using System.Collections.Generic;
    using Newtonsoft.Json;

    public class HitsCollection
    {
        private readonly List<Hit> hits = new List<Hit>();

        /// <summary>
        /// Gets the Total.
        /// </summary>
        [JsonProperty("total")]
        public int Total { get; private set; }

        /// <summary>
        /// Gets or sets MaxScore.
        /// </summary>
        [JsonProperty("max_score")]
        public object MaxScore { get; set; }

        /// <summary>
        /// Gets all hits.
        /// </summary>
        [JsonProperty("hits")]
        public IEnumerable<Hit> Hits
        {
            get { return hits; }
        }

        /// <summary>
        /// Add all hits.
        /// </summary>
        /// <param name="hits">IEnumerable.<Hit> collection of hits.</param>
        public void AddHits(IEnumerable<Hit> hits)
        {
            this.hits.AddRange(hits);
        }

        /// <summary>
        /// Calculating HitsCollection.Total by summing up the DocCount values of every DateHistogramBucket.
        /// </summary>
        /// <param name="docCount">int representing doc count.</param>
        public void AddToTotal(int docCount)
        {
            Total += docCount;
        }
    }
}
