// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

using K2Bridge.JsonConverters;
using Newtonsoft.Json;

namespace K2Bridge.Models.Response.Metadata;

/// <summary>
/// Field capability response element.
/// </summary>
[JsonConverter(typeof(FieldCapabilityElementConverter))]
public class FieldCapabilityElement
{
    /// <summary>
    /// Gets or sets field name.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Gets or sets field type.
    /// </summary>
    public string Type { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the field is aggregatable.
    /// </summary>
    public bool IsAggregatable { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether the field is searchable.
    /// </summary>
    public bool IsSearchable { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether the field is metadatas.
    /// </summary>
    public bool IsMetadataField { get; set; }
}
