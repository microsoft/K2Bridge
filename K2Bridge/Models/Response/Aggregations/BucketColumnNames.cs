// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2Bridge.Models.Response.Aggregations;

/// <summary>
/// ColumnNames.
/// </summary>
internal class BucketColumnNames
{
    /// <summary>
    /// Gets Count.
    /// </summary>
    public static string Count => "count_";

    /// <summary>
    /// Gets name of the default bucket for values that don't match any range.
    /// </summary>
    public static string RangeDefaultBucket => "default_bucket";
}
