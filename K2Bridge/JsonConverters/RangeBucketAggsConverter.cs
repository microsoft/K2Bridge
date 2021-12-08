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
    internal class RangeBucketAggsConverter : BucketAggsJsonConverter
    {
        /// <inheritdoc/>
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var buck = (RangeBucket)value;
            var aggs = buck.Aggs;

            writer.WriteStartObject();

            writer.WritePropertyName("doc_count");
            serializer.Serialize(writer, buck.DocCount);

            if (buck.From is not null)
            {
                writer.WritePropertyName("from");
                serializer.Serialize(writer, buck.From);
            }

            if (buck.To is not null)
            {
                writer.WritePropertyName("to");
                serializer.Serialize(writer, buck.To);
            }

            if (buck.Key is not null)
            {
                writer.WritePropertyName("key");
                serializer.Serialize(writer, buck.Key);
            }

            WriteAggregations(writer, aggs, serializer);

            writer.WriteEndObject();
        }
    }
}
