namespace K2Bridge.KustoConnector
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
    }
}
