// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2Bridge.Telemetry
{
    using Microsoft.ApplicationInsights;
    using Prometheus;

    /// <summary>
    /// Prometheus Histograms to collect query performance data.
    /// </summary>
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
        /// <param name="telemetryClient">The ApplicationInsights telemery client (null if AppInsights is off).</param>
        /// <returns>a MetricsHistograms.</returns>
        public static Metrics Create(TelemetryClient telemetryClient)
        {
            var metrics = new Metrics();

            var name = "adx_query_total_seconds";
            var help = "ADX query total execution time in seconds.";
            metrics.AdxQueryDurationMetric = new Histogram(
                Prometheus.Metrics.CreateHistogram(name, help, new HistogramConfiguration
                {
                    Buckets = Prometheus.Histogram.LinearBuckets(start: 1, width: 1, count: 60),
                }),
                name,
                help,
                telemetryClient?.GetMetric(name));

            name = "adx_query_net_seconds";
            help = "ADX query net execution time in seconds.";
            metrics.AdxNetQueryDurationMetric = new Histogram(
                Prometheus.Metrics.CreateHistogram(name, help, new HistogramConfiguration
                {
                    Buckets = Prometheus.Histogram.LinearBuckets(start: 1, width: 1, count: 60),
                }),
                name,
                help,
                telemetryClient?.GetMetric(name));

            name = "adx_query_result_bytes";
            help = "ADX query result payload size in bytes.";
            metrics.AdxQueryBytesMetric = new Histogram(
                Prometheus.Metrics.CreateHistogram(name, help, new HistogramConfiguration
                {
                    Buckets = Prometheus.Histogram.LinearBuckets(start: 1, width: 250000, count: 40),
                }),
                name,
                help,
                telemetryClient?.GetMetric(name));

            return metrics;
        }
    }
}
