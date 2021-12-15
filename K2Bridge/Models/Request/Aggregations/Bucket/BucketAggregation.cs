// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2Bridge.Models.Request.Aggregations
{
    /// <summary>
    /// Bucket aggregations calculate metrics by creating buckets of documents.
    /// </summary>
    public abstract class BucketAggregation : Aggregation
    {
        public string Metric { get; internal set; } = "count()";
    }
}
