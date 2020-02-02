// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2Bridge
{
    using Microsoft.ApplicationInsights.Channel;
    using Microsoft.ApplicationInsights.DataContracts;
    using Microsoft.ApplicationInsights.Extensibility;

    /// <summary>
    /// This class adds identifiable information to each AppInsights telemetry.
    /// </summary>
    internal class TelemetryInitializer : ITelemetryInitializer
    {
        private string identifier;

        public TelemetryInitializer(string identifier) {
          this.identifier = identifier;
        }

        public void Initialize(ITelemetry telemetry)
        {
            var itemProperties = telemetry as ISupportProperties;
            if (itemProperties != null)
            {
                itemProperties.Properties["k2-identifier"] = identifier;
            }
        }
    }
}