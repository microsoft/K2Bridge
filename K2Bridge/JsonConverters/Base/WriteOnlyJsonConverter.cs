// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2Bridge.JsonConverters.Base;

using System;
using Newtonsoft.Json;

/// <summary>
/// An abstract class used to implement the ReadJson functionality of a JsonConverter
/// without the need to override the additional methods of the base class.
/// </summary>
internal abstract class WriteOnlyJsonConverter : JsonConverter
{
    /// <inheritdoc/>
    public override bool CanRead
    {
        get { return false; }
    }

    /// <inheritdoc/>
    public override bool CanConvert(Type objectType)
    {
        // We don't really need this since we're always using the 'JsonConverter' property
        throw new NotImplementedException();
    }

    /// <inheritdoc/>
    public override object ReadJson(
        JsonReader reader,
        Type objectType,
        object existingValue,
        JsonSerializer serializer)
    {
        throw new NotImplementedException();
    }
}
