// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2Bridge.Models.Request.Aggregations
{
    /// <summary>
    /// Bucket aggregations calculate metrics by creating buckets of documents.
    /// </summary>
    internal abstract class BucketAggregation : Aggregation
    {
        /// <summary>
        /// Gets or sets the bucket aggregation metric.
        /// </summary>
        public string Metric { get; set; } = "count()";

        /// <summary>
        /// Gets or sets the summarizable metrics part of the query, assembled by the sub aggregations.
        /// </summary>
        public string SummarizableMetricsKustoQL { get; set; }

        /// <summary>
        /// Gets or sets the partionable metrics part of the query, assembled by the sub aggregations.
        /// </summary>
        public string PartitionableMetricsKustoQL { get; set; }
    }
}
