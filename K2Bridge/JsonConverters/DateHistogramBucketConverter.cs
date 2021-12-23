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
    /// This converter serializes <see cref="DateHistogramBucket"/> to Elasticsearh response json format.
    /// </summary>
    internal class DateHistogramBucketConverter : WriteOnlyJsonConverter
    {
        /// <inheritdoc/>
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            writer.WriteStartObject();

            if (value is DateHistogramBucket dateHistogramBucket)
            {
                writer.WritePropertyName("doc_count");
                serializer.Serialize(writer, dateHistogramBucket.DocCount);

                writer.WritePropertyName("key");
                serializer.Serialize(writer, dateHistogramBucket.Key);

                if (dateHistogramBucket.KeyAsString is not null)
                {
                    writer.WritePropertyName("key_as_string");
                    serializer.Serialize(writer, dateHistogramBucket.KeyAsString);
                }

                foreach (KeyValuePair<string, IAggregate> aggregate in dateHistogramBucket)
                {
                    writer.WritePropertyName(aggregate.Key);
                    serializer.Serialize(writer, aggregate.Value);
                }
            }

            writer.WriteEndObject();
        }
    }
}