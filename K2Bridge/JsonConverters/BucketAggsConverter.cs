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
    internal class BucketAggsConverter : BucketAggsJsonConverter
    {
        /// <inheritdoc/>
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var bucket = (Bucket)value;
            var aggs = bucket.Aggs;

            writer.WriteStartObject();

            writer.WritePropertyName("doc_count");
            serializer.Serialize(writer, bucket.DocCount);
            writer.WritePropertyName("key");
            serializer.Serialize(writer, bucket.Key);

            WriteAggregations(writer, aggs, serializer);

            writer.WriteEndObject();
        }
    }
}
