// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2Bridge.Utils
{
    public static class AggregationsConstants
    {
        /// <summary>
        /// Separator character used to join and split column metadata
        /// Only used for percentiles metrics at the moment
        /// </summary>
        public const char MetadataSeparator = '%';
        public const string Average = "average";
        public const string StandardDeviation = "std_deviation";
        public const string StandardDeviationBoundsUpper = "std_deviation_bounds_upper";
        public const string StandardDeviationBoundsLower = "std_deviation_bounds_lower";
    }
}