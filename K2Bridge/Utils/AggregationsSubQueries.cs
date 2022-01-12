// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2Bridge.Utils
{
    public static class AggregationsSubQueries
    {
        /// <summary>
        /// Sub query name used in <see cref="BuildExtendDataQuery"/>.
        /// </summary>
        public const string ExtDataQuery = "_extdata";

        /// <summary>
        /// Sub query name used in <see cref="BuildSummarizableMetricsQuery"/>.
        /// </summary>
        public const string SummarizableMetricsQuery = "_summarizablemetrics";
    }
}