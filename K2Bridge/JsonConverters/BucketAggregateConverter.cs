// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2Bridge.JsonConverters
{
    using System.Linq;
    using K2Bridge.JsonConverters.Base;
    using K2Bridge.Models.Response.Aggregations;
    using Newtonsoft.Json;

    /// <summary>
    /// This converter serializes <see cref="BucketAggregate"/> to Elasticsearh response json format.
    /// </summary>
    internal class BucketAggregateConverter : WriteOnlyJsonConverter
    {
        /// <inheritdoc/>
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            writer.WriteStartObject();
            writer.WritePropertyName("buckets");

            if (value is BucketAggregate<RangeBucket> rangeAggregate)
            {
                KeyedSerialization<RangeBucket, string>(writer, rangeAggregate, serializer);
            }

            if (value is BucketAggregate<DateHistogramBucket> dateHistogramAggregate)
            {
                serializer.Serialize(writer, dateHistogramAggregate.Buckets);
            }

            if (value is BucketAggregate<TermsBucket> termsAggregate)
            {
                serializer.Serialize(writer, termsAggregate.Buckets);
            }

            writer.WriteEndObject();
        }

        private void KeyedSerialization<TBucket, TKey>(JsonWriter writer, BucketAggregate<TBucket> aggregate, JsonSerializer serializer)
            where TBucket : KeyedBucket<TKey>
        {
            // Setting the keyed flag to true will associate a unique string key with each bucket and return the ranges as a hash rather than an array.
            if (aggregate.Keyed)
            {
                serializer.Serialize(writer, aggregate.Buckets.ToDictionary(
                    bucket => bucket.Key,
                    bucket =>
                    {
                        bucket.Key = default(TKey);
                        return bucket;
                    }));
            }
            else
            {
                serializer.Serialize(writer, aggregate.Buckets);
            }
        }
    }
}