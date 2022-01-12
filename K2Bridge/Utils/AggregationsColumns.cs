﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2Bridge.Utils
{
    /// <summary>
    /// ColumnNames.
    /// </summary>
    public class AggregationsColumns
    {
        /// <summary>
        /// Gets count column name.
        /// </summary>
        public static string Count => "count_";

        /// <summary>
        /// Gets Percentile column name.
        /// </summary>
        public static string Percentile => "percentile";

        /// <summary>
        /// Gets TopHits column name.
        /// </summary>
        public static string TopHits => "tophits";

        /// <summary>
        /// Gets name of the default bucket for values that don't match any range.
        /// </summary>
        public static string RangeDefaultBucket => "default_bucket";
    }
}