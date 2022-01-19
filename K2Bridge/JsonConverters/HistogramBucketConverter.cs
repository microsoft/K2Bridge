// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2Bridge.JsonConverters
{
    using System.Collections.Generic;
    using K2Bridge.JsonConverters.Base;
    using K2Bridge.Models.Response.Aggregations;
    using Newtonsoft.Json;

    /// <summary>
    /// A converter able to serialize Bucket aggregations to Elasticsearh response json format.
    /// </summary>
    internal class HistogramBucketConverter : WriteOnlyJsonConverter
    {
        /// <inheritdoc/>
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var histogramBucket = (HistogramBucket)value;

            writer.WriteStartObject();

            writer.WritePropertyName("doc_count");
            serializer.Serialize(writer, histogramBucket.DocCount);

            if (histogramBucket.Key is not null)
            {
                writer.WritePropertyName("key");
                serializer.Serialize(writer, histogramBucket.Key);
            }

            if (histogramBucket.KeyAsString is not null)
            {
                writer.WritePropertyName("key_as_string");
                serializer.Serialize(writer, histogramBucket.KeyAsString);
            }

            foreach (var (key, aggregate) in histogramBucket)
            {
                writer.WritePropertyName(key);
                serializer.Serialize(writer, aggregate);
            }

            writer.WriteEndObject();
        }
    }
}
