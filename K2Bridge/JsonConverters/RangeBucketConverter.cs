// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

using K2Bridge.JsonConverters.Base;
using K2Bridge.Models.Response.Aggregations.Bucket;
using Newtonsoft.Json;

namespace K2Bridge.JsonConverters;

/// <summary>
/// This converter serializes <see cref="RangeBucket"/> to Elasticsearh response json format.
/// </summary>
internal class RangeBucketConverter : WriteOnlyJsonConverter
{
    /// <inheritdoc/>
    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
        var rangeBucket = (RangeBucket)value;

        writer.WriteStartObject();

        writer.WritePropertyName("doc_count");
        serializer.Serialize(writer, rangeBucket.DocCount);

        if (rangeBucket.Key is not null)
        {
            writer.WritePropertyName("key");
            serializer.Serialize(writer, rangeBucket.Key);
        }

        if (rangeBucket.From is not null)
        {
            writer.WritePropertyName("from");
            serializer.Serialize(writer, rangeBucket.From);
        }

        if (rangeBucket.FromAsString is not null)
        {
            writer.WritePropertyName("from_as_string");
            serializer.Serialize(writer, rangeBucket.FromAsString);
        }

        if (rangeBucket.To is not null)
        {
            writer.WritePropertyName("to");
            serializer.Serialize(writer, rangeBucket.To);
        }

        if (rangeBucket.ToAsString is not null)
        {
            writer.WritePropertyName("to_as_string");
            serializer.Serialize(writer, rangeBucket.ToAsString);
        }

        foreach (var aggregate in rangeBucket)
        {
            writer.WritePropertyName(aggregate.Key);
            serializer.Serialize(writer, aggregate.Value);
        }

        writer.WriteEndObject();
    }
}
