// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

using K2Bridge.JsonConverters;
using Newtonsoft.Json;

namespace K2Bridge.Models.Response.Aggregations;

/// <summary>
/// Describes value aggregate response element.
/// </summary>
[JsonConverter(typeof(ValueAggregateConverter))]
public class ValueAggregate : IAggregate
{
    /// <summary>
    /// Gets or sets aggregate Value.
    /// </summary>
    public double? Value { get; set; }

    /// <summary>
    /// Gets or sets aggregate ValueAsString.
    /// </summary>
    public string ValueAsString { get; set; }
}
