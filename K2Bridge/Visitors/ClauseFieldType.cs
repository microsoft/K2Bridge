// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2Bridge.Visitors;

/// <summary>
/// ClauseFieldType.
/// </summary>
public enum ClauseFieldType
{
    /// <summary>
    /// Numeric value type
    /// </summary>
    Numeric,

    /// <summary>
    /// String value type
    /// </summary>
    Text,

    /// <summary>
    /// Date type
    /// </summary>
    Date,

    /// <summary>
    /// Unknown type
    /// </summary>
    Unknown,
}
