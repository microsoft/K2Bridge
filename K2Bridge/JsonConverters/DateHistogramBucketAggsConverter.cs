﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2Bridge.JsonConverters
{
    using K2Bridge.JsonConverters.Base;
    using K2Bridge.Models.Response;
    using System.Linq;
    using Newtonsoft.Json;
    using System;

    /// <summary>
    /// A converter able to serialize Bucket aggregations to Elasticsearh response json format.
    /// </summary>
    internal class DateHistogramBucketAggsConverter : WriteOnlyJsonConverter
    {
        /// <inheritdoc/>
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var buck = value as DateHistogramBucket;
            var aggs = buck.Aggs;

            writer.WriteStartObject();

            writer.WritePropertyName("doc_count");
            serializer.Serialize(writer, buck.DocCount);
            writer.WritePropertyName("key");
            serializer.Serialize(writer, buck.Key);
            writer.WritePropertyName("key_as_string");
            serializer.Serialize(writer, buck.KeyAsString);

            foreach (var agg in aggs.Keys)
            {
                writer.WritePropertyName(agg);

                writer.WriteStartObject();
                if (aggs[agg].Count > 1)
                {
                    writer.WritePropertyName("values");
                    serializer.Serialize(writer, aggs[agg].ToDictionary(item => Convert.ToDouble(item.Key), item => Convert.ToDouble(item.Value)).ToList());
                }
                else
                {
                    var item = aggs[agg].First();

                    if ("value".Equals(item.Key))
                    {
                        writer.WritePropertyName("value");
                        serializer.Serialize(writer, item.Value);
                    }
                    else
                    {
                        writer.WritePropertyName("values");
                        serializer.Serialize(writer, aggs[agg]);
                    }
                }

                writer.WriteEndObject();
            }

            writer.WriteEndObject();
        }
    }
}
