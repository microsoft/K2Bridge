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
    internal class AggregationsConverter : WriteOnlyJsonConverter
    {
        /// <inheritdoc/>
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var aggs = value as Aggregations;

            writer.WriteStartObject();

            writer.WritePropertyName(aggs.Parent);
            serializer.Serialize(writer, aggs.Collection);

            writer.WriteEndObject();
        }
    }
}
