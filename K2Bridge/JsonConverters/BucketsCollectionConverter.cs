// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2Bridge.JsonConverters
{
    using K2Bridge.JsonConverters.Base;
    using K2Bridge.Models.Response;
    using Newtonsoft.Json;

    /// <summary>
    /// A converter able to serialize <see cref="Aggregations"/> to Elasticsearh response json format.
    /// </summary>
    internal class BucketsCollectionConverter : WriteOnlyJsonConverter
    {
        /// <inheritdoc/>
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var buckets = value as BucketsCollection;

            writer.WriteStartObject();
            writer.WritePropertyName("buckets");

            if (buckets.Keyed)
            {
                // A keyed bucket collection is returned as a hash, with the keys as the property names
                writer.WriteStartObject();

                foreach (var bucket in buckets.Buckets)
                {
                    writer.WritePropertyName(bucket.Key);

                    // Do not output the key in the bucket itself
                    bucket.Key = null;

                    serializer.Serialize(writer, bucket);
                }

                writer.WriteEndObject();
            }
            else
            {
                // A non-keyed bucket collection is returned as an array
                serializer.Serialize(writer, buckets.Buckets);
            }

            writer.WriteEndObject();
        }
    }
}
