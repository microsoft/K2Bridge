// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

using System.Collections;
using System.Threading.Tasks;

namespace K2Bridge.KustoDAL;

/// <summary>
/// This is used to fetch the actual schema of a given
/// table. It is used in the visitors path, while building the
/// generated query in order to give an accurate translation.
/// </summary>
public interface ISchemaRetriever
{
    /// <summary>
    /// Gets the index name.
    /// </summary>
    string IndexName { get; }

    /// <summary>
    /// Retrieves table schema.
    /// </summary>
    /// <returns>Task.<IDictionary> with a table schema.</returns>
    Task<IDictionary> RetrieveTableSchema();
}
