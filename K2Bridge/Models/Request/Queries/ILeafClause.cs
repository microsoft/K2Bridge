// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

using K2Bridge.JsonConverters;
using Newtonsoft.Json;

namespace K2Bridge.Models.Request.Queries;

/// <summary>
/// Leaf clause to visit.
/// </summary>
[JsonConverter(typeof(LeafClauseConverter))]
internal interface ILeafClause : IQuery
{
    string KustoQL { get; set; }
}
