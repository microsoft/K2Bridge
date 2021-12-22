// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2Bridge.JsonConverters
{
    using System.Globalization;
    using System.Linq;
    using K2Bridge.JsonConverters.Base;
    using K2Bridge.Models.Response.Aggregations;
    using Newtonsoft.Json;

    /// <summary>
    /// This converter serializes <see cref="PercentileAggregate"/> to Elasticsearh response json format.
    /// </summary>
    internal class PercentileAggregateConverter : WriteOnlyJsonConverter
    {
        /// <inheritdoc/>
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var aggregate = value as PercentileAggregate;

            writer.WriteStartObject();
            writer.WritePropertyName("values");

            if (aggregate.Keyed)
            {
                // Keyed ==> Dictionary<string, double>
                serializer.Serialize(writer, aggregate.Values.ToDictionary(item => item.Percentile.ToString("F1", CultureInfo.InvariantCulture), item => item.Value));
            }
            else
            {
                // Not Keyed ==> List<KeyValuePair<double,double>>
                serializer.Serialize(writer, aggregate.Values.ToDictionary(item => item.Percentile, item => item.Value).ToList());
            }

            writer.WriteEndObject();
        }
    }
}