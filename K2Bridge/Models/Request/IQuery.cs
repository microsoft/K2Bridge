// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

using K2Bridge.JsonConverters;
using Newtonsoft.Json;

namespace K2Bridge.Models.Request;

/// <summary>
/// An interface for query class.
/// </summary>
[JsonConverter(typeof(IQueryConverter))]
internal interface IQuery
{
}
