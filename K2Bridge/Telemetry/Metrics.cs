// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2Bridge.Telemetry
{
    using Prometheus;

    /// <summary>
    /// Prometheus Histograms to collect query performance data.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1724", Justification ="Metrics is a valid name.")]
    public class Metrics
    {
        /// <summary>
        /// Gets or Sets AdxQueryDurationMetric.
        /// </summary>
        public Histogram AdxQueryDurationMetric { get; set; }

        /// <summary>
        /// Gets or Sets AdxNetQueryDurationMetric.
        /// </summary>
        public Histogram AdxNetQueryDurationMetric { get; set; }

        /// <summary>
        /// Gets or Sets AdxQueryBytesMetric.
        /// </summary>
        public Histogram AdxQueryBytesMetric { get; set; }

        /// <summary>
        /// A factory function.
        /// </summary>
        /// <returns>a MetricsHistograms.</returns>
        public static Metrics Create()
        {
            var mh = new Metrics();

            var name = "adx_query_total_seconds";
            var help = "ADX query total execution time in seconds.";
            mh.AdxQueryDurationMetric = new Histogram(
                Prometheus.Metrics.CreateHistogram(name, help, new HistogramConfiguration
                {
                    Buckets = Prometheus.Histogram.LinearBuckets(start: 1, width: 1, count: 60),
                }),
                name,
                help);

            name = "adx_query_net_seconds";
            help = "ADX query net execution time in seconds.";
            mh.AdxNetQueryDurationMetric = new Histogram(
                Prometheus.Metrics.CreateHistogram(name, help, new HistogramConfiguration
                {
                    Buckets = Prometheus.Histogram.LinearBuckets(start: 1, width: 1, count: 60),
                }),
                name,
                help);

            name = "adx_query_result_bytes";
            help = "ADX query result payload size in bytes.";
            mh.AdxQueryBytesMetric = new Histogram(
                Prometheus.Metrics.CreateHistogram(name, help, new HistogramConfiguration
                {
                    Buckets = Prometheus.Histogram.LinearBuckets(start: 1, width: 250000, count: 40),
                }),
                name,
                help);

            return mh;
        }
    }
}
