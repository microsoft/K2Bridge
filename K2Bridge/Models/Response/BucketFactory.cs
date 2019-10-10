namespace K2Bridge.Models.Response
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