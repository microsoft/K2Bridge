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
    internal class DateRangeBucketConverter : WriteOnlyJsonConverter
    {
        /// <inheritdoc/>
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var bucket = (DateRangeBucket)value;

            writer.WriteStartObject();

            writer.WritePropertyName("doc_count");
            serializer.Serialize(writer, bucket.DocCount);

            if (bucket.From is not null)
            {
                writer.WritePropertyName("from");

                // ES formats this long as a double (trailing zero)
                serializer.Serialize(writer, (double)bucket.From);
            }

            if (bucket.FromAsString is not null)
            {
                writer.WritePropertyName("from_as_string");
                serializer.Serialize(writer, bucket.FromAsString);
            }

            if (bucket.To is not null)
            {
                writer.WritePropertyName("to");

                // ES formats this long as a double (trailing zero)
                serializer.Serialize(writer, (double)bucket.To);
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

            foreach (KeyValuePair<string, IAggregate> aggregate in bucket)
            {
                writer.WritePropertyName(aggregate.Key);
                serializer.Serialize(writer, aggregate.Value);
            }

            writer.WriteEndObject();
        }
    }
}
