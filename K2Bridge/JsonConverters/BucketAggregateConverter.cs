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
            var bucketAggregate = (BucketAggregate)value;

            writer.WriteStartObject();
            writer.WritePropertyName("buckets");

            // Setting the keyed flag to true will associate a unique string key with each bucket and return the ranges as a hash rather than an array.
            if (bucketAggregate.Keyed)
            {
                serializer.Serialize(writer, bucketAggregate.Buckets.ToDictionary(
                    bucket => bucket.Key,
                    bucket =>
                    {
                        bucket.Key = null;
                        return bucket;
                    }));
            }
            else
            {
                serializer.Serialize(writer, bucketAggregate.Buckets);
            }

            writer.WriteEndObject();
        }
    }
}