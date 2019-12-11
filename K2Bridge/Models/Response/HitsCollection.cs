// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace K2Bridge.Models.Response
{
    using System.Collections.Generic;
    using Newtonsoft.Json;

    public class HitsCollection
    {
        private List<Hit> hits = new List<Hit>();

        [JsonProperty("total")]
        public int Total { get; private set; }

        [JsonProperty("max_score")]
        public object MaxScore { get; set; }

        [JsonProperty("hits")]
        public IEnumerable<Hit> Hits
        {
            get { return this.hits; }
        }

        public void AddHit(Hit hit)
        {
            this.hits.Add(hit);
        }

        // Calculating HitsCollection.Total by summing up the DocCount values of every DateHistogramBucket
        public void AddToTotal(int docCount)
        {
            this.Total += docCount;
        }
    }
}
