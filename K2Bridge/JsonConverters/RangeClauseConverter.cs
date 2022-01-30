// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2Bridge.JsonConverters;

using System;
using System.Globalization;
using K2Bridge.JsonConverters.Base;
using K2Bridge.Models.Request.Queries;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

/// <summary>
/// A converter able to deserialize the range element in an Elasticsearch query to <see cref="RangeClause"/>.
/// </summary>
internal class RangeClauseConverter : ReadOnlyJsonConverter
{
    /// <inheritdoc/>
    public override object ReadJson(
        JsonReader reader,
        Type objectType,
        object existingValue,
        JsonSerializer serializer)
    {
        var jo = JObject.Load(reader);
        var first = (JProperty)jo.First;

        var obj = new RangeClause
        {
            FieldName = first.Name,
            GTEValue = ConvertToString(first, "gte"),
            GTValue = ConvertToString(first, "ge"),
            LTEValue = ConvertToString(first, "lte"),
            LTValue = ConvertToString(first, "lt"),
            Format = first.First.Value<string>("format"),
        };

        return obj;
    }

    private static string ConvertToString(JToken prop, string value)
    {
        var first = prop.First[value];
        if (first == null)
        {
            return null;
        }

        return first.Type switch
        {
            JTokenType.Date => first.Value<DateTime>().ToString("o"),
            JTokenType.Float => first.Value<double>().ToString(CultureInfo.InvariantCulture),
            JTokenType.Integer => first.Value<long>().ToString(CultureInfo.InvariantCulture),
            _ => first.Value<string>(value),
        };
    }
}
