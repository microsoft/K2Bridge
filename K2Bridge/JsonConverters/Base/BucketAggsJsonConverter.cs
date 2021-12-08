// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2Bridge.JsonConverters.Base
{
    using System;
    using System.Collections.Generic;
    using Newtonsoft.Json;

    /// <summary>
    /// A base class for bucket aggregations converters.
    /// </summary>
    internal abstract class BucketAggsJsonConverter : WriteOnlyJsonConverter
    {
        public void WriteAggregations(JsonWriter writer, Dictionary<string, List<double>> aggs, JsonSerializer serializer)
        {
            foreach (var agg in aggs.Keys)
            {
                writer.WritePropertyName(agg);

                writer.WriteStartObject();
                if (aggs[agg].Count > 1)
                {
                    writer.WritePropertyName("values");
                    serializer.Serialize(writer, aggs[agg]);
                }
                else
                {
                    writer.WritePropertyName("value");
                    var item = aggs[agg][0];
                    serializer.Serialize(writer, item);
                }

                writer.WriteEndObject();
            }
        }
    }
}
