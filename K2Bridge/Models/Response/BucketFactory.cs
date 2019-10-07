namespace K2Bridge.KustoConnector
{
    internal static class BucketFactory
    {
        // TODO: implement more aggregation buckets
        public static IBucket MakeBucket(System.Data.IDataRecord record)
        {
            return DateHistogramBucket.Create(record);
        }
    }
}