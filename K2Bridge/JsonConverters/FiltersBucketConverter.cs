// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2Bridge.JsonConverters;

using K2Bridge.JsonConverters.Base;
using K2Bridge.Models.Response.Aggregations.Bucket;
using Newtonsoft.Json;

/// <summary>
/// This converter serializes <see cref="FiltersBucket"/> to Elasticsearh response json format.
/// </summary>
internal class FiltersBucketConverter : WriteOnlyJsonConverter
{
    /// <inheritdoc/>
    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
        var filtersBucket = (FiltersBucket)value;

        writer.WriteStartObject();

        writer.WritePropertyName("doc_count");
        serializer.Serialize(writer, filtersBucket.DocCount);

        if (filtersBucket.Key is not null)
        {
            writer.WritePropertyName("key");
            serializer.Serialize(writer, filtersBucket.Key);
        }

        foreach (var (key, aggregate) in filtersBucket)
        {
            writer.WritePropertyName(key);
            serializer.Serialize(writer, aggregate);
        }

        writer.WriteEndObject();
    }
}
