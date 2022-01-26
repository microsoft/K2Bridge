// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2Bridge.Models.Request.Aggregations
{
    using K2Bridge.Visitors;
    using Newtonsoft.Json;

    /// <summary>
    /// This multi-bucket aggregation is similar to the normal histogram, but it can only be used with date or date range values.
    /// </summary>
    internal class DateHistogramAggregation : BucketAggregation
    {
        /// <summary>
        /// Gets or sets the field to target.
        /// </summary>
        [JsonProperty("field")]
        public string Field { get; set; }

        /// <summary>
        /// Gets or sets the calendar interval to use when bucketing documents.
        /// </summary>
        [JsonProperty("calendar_interval")]
        public string CalendarInterval { get; set; }

        /// <summary>
        /// Gets or sets the fixed interval to use when bucketing documents.
        /// </summary>
        [JsonProperty("fixed_interval")]
        public string FixedInterval { get; set; }

        /// <summary>
        /// Gets or sets the minimum number of documents that a bucket must contain to be returned in the response.
        /// The default is 0 meaning that buckets with no documents will be returned.
        /// </summary>
        [JsonProperty("min_doc_count")]
        public int? MinimumDocumentCount { get; set; }

        /// <summary>
        /// Gets or sets the time zone.
        /// Used to indicate that bucketing should use a different time zone.
        /// Time zones may either be specified as an ISO 8601 UTC offset (e.g. +01:00 or -08:00)
        /// or as a timezone id, an identifier used in the TZ database like America/Los_Angeles.
        /// </summary>
        [JsonProperty("time_zone")]
        public string TimeZone { get; set; }

        /// <inheritdoc/>
        public override void Accept(IVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
}
