// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2Bridge.Utils;

/// <summary>
/// Holds the table names used within Kusto/ADX Queries.
/// </summary>
public static class KustoTableNames
{
    /// <summary>
    /// The filtered data table name.
    /// Should only used internally in the query and not returned to the client as is.
    /// </summary>
    public const string Data = "_data";

    /// <summary>
    /// The aggregations data table name.
    /// </summary>
    public const string Aggregation = "aggs";

    /// <summary>
    /// The hits (raw items) data table name.
    /// </summary>
    public const string Hits = "hits";

    /// <summary>
    /// The overall hits count data table name.
    /// </summary>
    public const string HitsTotal = "hitsTotal";

    /// <summary>
    /// The metadata table.
    /// </summary>
    public const string Metadata = "metadata";

    /// <summary>
    /// The data type for the metadata columns.
    /// </summary>
    public const string MetadataColumnType = "string";

    /// <summary>
    /// The metadata key column name.
    /// </summary>
    public const string MetadataKey = "key";

    /// <summary>
    /// The metadata value column name.
    /// </summary>
    public const string MetadataValue = "value";
}
