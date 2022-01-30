// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2Bridge.JsonConverters
{
    using K2Bridge.JsonConverters.Base;
    using K2Bridge.Models.Response.Aggregations.Bucket;
    using Newtonsoft.Json;

    /// <summary>
    /// This converter serializes <see cref="DateHistogramBucket"/> to Elasticsearh response json format.
    /// </summary>
    internal class DateHistogramBucketConverter : WriteOnlyJsonConverter
    {
        /// <inheritdoc/>
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var dateHistogramBucket = (DateHistogramBucket)value;

            writer.WriteStartObject();

            writer.WritePropertyName("doc_count");
            serializer.Serialize(writer, dateHistogramBucket.DocCount);

            if (dateHistogramBucket.Key is not null)
            {
                writer.WritePropertyName("key");
                serializer.Serialize(writer, dateHistogramBucket.Key);
            }

            if (dateHistogramBucket.KeyAsString is not null)
            {
                writer.WritePropertyName("key_as_string");
                serializer.Serialize(writer, dateHistogramBucket.KeyAsString);
            }

            foreach (var aggregate in dateHistogramBucket)
            {
                writer.WritePropertyName(aggregate.Key);
                serializer.Serialize(writer, aggregate.Value);
            }

            writer.WriteEndObject();
        }
    }
}
