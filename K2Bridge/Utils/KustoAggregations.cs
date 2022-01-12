// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2Bridge.Utils
{
    public static class KustoAggregations
    {
        /// <summary>
        /// Separator character used to join and split column metadata.
        /// </summary>
        public const char MetadataSeparator = '%';

        /// <summary>
        /// Identifier name used in <see cref="TopHitsAggregation"/> visitor.
        /// </summary>
        public const string TopHits = "tophits";

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