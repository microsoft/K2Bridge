// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

using K2Bridge.Models.Response;

namespace K2Bridge.Factories;

/// <summary>
/// Hits Factory.
/// </summary>
public static class HitsFactory
{
    /// <summary>
    /// Creates a Hit.
    /// </summary>
    /// <param name="id">Id.</param>
    /// <param name="indexName">Index name.</param>
    /// <returns>Hit.</returns>
    public static Hit Create(string id, string indexName)
    => new() { Id = id, Index = indexName };
}
