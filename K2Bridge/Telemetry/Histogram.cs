// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2Bridge.Telemetry
{
    using Microsoft.ApplicationInsights;
    using Prometheus;

    /// <summary>
    /// Encapsulation for Metric Histogram.
    /// </summary>
    public class Histogram
    {
        private readonly IHistogram histogram;
        private readonly Metric appInsightsMetric;

        /// <summary>
        /// Initializes a new instance of the <see cref="Histogram"/> class.
        /// </summary>
        public Histogram()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Histogram"/> class.
        /// </summary>
        /// <param name="histogram">A histogram.</param>
        /// <param name="name">A name.</param>
        /// <param name="help">Help text.</param>
        /// <param name="appInsightsMetric">The ApplicationInsights <see cref="Metric"/> object.</param>
        public Histogram(IHistogram histogram, string name, string help, Metric appInsightsMetric)
        {
            this.histogram = histogram;
            Name = name;
            Help = help;
            this.appInsightsMetric = appInsightsMetric;
        }

        /// <summary>
        /// Gets Name.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Gets help.
        /// </summary>
        public string Help { get; private set; }

        /// <summary>
        /// Observes a single event with the given value.
        /// </summary>
        /// <param name="val">The Value.</param>
        public void Observe(double val)
        {
            histogram.Observe(val);

            // AppInsights might not be on and the metric could be null
            appInsightsMetric?.TrackValue(val);
        }
    }
}
