// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2Bridge.JsonConverters
{
    using K2Bridge.JsonConverters.Base;
    using K2Bridge.Models.Response;
    using Newtonsoft.Json;

    /// <summary>
    /// A converter able to serialize Bucket aggregations to Elasticsearh response json format.
    /// </summary>
    internal class DateRangeBucketAggsConverter : BucketAggsJsonConverter
    {
        /// <inheritdoc/>
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var bucket = (DateRangeBucket)value;
            var aggs = bucket.Aggs;

            writer.WriteStartObject();

            writer.WritePropertyName("doc_count");
            serializer.Serialize(writer, bucket.DocCount);

            if (bucket.From is not null)
            {
                writer.WritePropertyName("from");
                serializer.Serialize(writer, bucket.From);
            }

            if (bucket.FromAsString is not null)
            {
                writer.WritePropertyName("from_as_string");
                serializer.Serialize(writer, bucket.FromAsString);
            }

            if (bucket.To is not null)
            {
                writer.WritePropertyName("to");
                serializer.Serialize(writer, bucket.To);
            }

            if (bucket.ToAsString is not null)
            {
                writer.WritePropertyName("to_as_string");
                serializer.Serialize(writer, bucket.ToAsString);
            }

            if (bucket.Key is not null)
            {
                writer.WritePropertyName("key");
                serializer.Serialize(writer, bucket.Key);
            }

            WriteAggregations(writer, aggs, serializer);

            writer.WriteEndObject();
        }
    }
}
