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
        public void WriteAggregations(JsonWriter writer, Dictionary<string, Dictionary<string, object>> aggs, JsonSerializer serializer)
        {
            foreach (var agg in aggs.Keys)
            {
                writer.WritePropertyName(agg);
                serializer.Serialize(writer, aggs[agg]);
            }
        }
    }
}
