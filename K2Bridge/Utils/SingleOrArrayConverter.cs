// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.
namespace K2Bridge.Utils;

using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

class SingleOrArrayConverter<T> : JsonConverter
{
    /// <inheritdoc/>
    public override bool CanWrite
    {
        get { return false; }
    }

    public override bool CanConvert(Type objectType)
    {
        return (objectType == typeof(List<T>)) || (objectType == typeof(IEnumerable<T>));
    }

    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    {
        JToken token = JToken.Load(reader);
        if (token.Type == JTokenType.Array)
        {
            return token.ToObject<List<T>>();
        }
        return new List<T> { token.ToObject<T>() };
    }

    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
        throw new NotImplementedException();
    }

}
