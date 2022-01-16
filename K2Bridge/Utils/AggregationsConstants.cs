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

        /// <summary>
        /// Alias used to get a filed named count, from adx
        /// </summary>
        public const string Count = "count";

        /// <summary>
        /// Alias used to get a filed named avg, from adx
        /// </summary>
        public const string Average = "avg";

        /// <summary>
        /// Alias used to get a filed named min, from adx
        /// </summary>
        public const string Min = "min";

        /// <summary>
        /// Alias used to get a filed named max, from adx
        /// </summary>
        public const string Max = "max";

        /// <summary>
        /// Alias used to get a filed named sum, from adx
        /// </summary>
        public const string Sum = "sum";

        /// <summary>
        /// Alias used to get a filed named sum_of_squares, from adx
        /// </summary>
        public const string SumOfSquares = "sum_of_squares";

        /// <summary>
        /// Alias used to get a filed named variance_population, from adx
        /// </summary>
        public const string VariancePopulation = "variance_population";

        /// <summary>
        /// Alias used to get a filed named variance_sampling, from adx
        /// </summary>
        public const string VarianceSampling = "variance_sampling";

        /// <summary>
        /// Alias used to get a filed named std_deviation_population, from adx
        /// </summary>
        public const string StandardDeviationPopulation = "std_deviation_population";

        /// <summary>
        /// Alias used to get a filed named std_deviation_sampling, from adx
        /// </summary>
        public const string StandardDeviationSampling = "std_deviation_sampling";

        /// <summary>
        /// Gets ExtendedStats column metadata name.
        /// </summary>
        public const string ExtendedStats = "extended_stats";

        /// <summary>
        /// Gets Percentile column metadata name.
        /// </summary>
        public const string Percentile = "percentile";

        /// <summary>
        /// Gets TopHits column metadata name.
        /// </summary>
        public const string TopHits = "tophits";

        /// <summary>
        /// Gets count (default bucket metric key).
        /// </summary>
        public const string CountKey = "count_";
    }
}