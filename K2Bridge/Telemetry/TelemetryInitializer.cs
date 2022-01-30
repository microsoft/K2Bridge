// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

using System;
using Microsoft.ApplicationInsights.AspNetCore.TelemetryInitializers;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.AspNetCore.Http;

namespace K2Bridge.Telemetry;

/// <summary>
/// This class adds identifiable information to each AppInsights telemetry.
/// </summary>
internal class TelemetryInitializer : TelemetryInitializerBase
{
    private const string SyntheticSourceHeaderValue = "Availability Monitoring";

    private const string K2IdentifierPropertyName = "k2-identifier";
    private readonly string identifier;
    private readonly string healthCheckRoute;

    /// <summary>
    /// Initializes a new instance of the <see cref="TelemetryInitializer"/> class.
    /// </summary>
    /// <param name="httpContextAccessor">The <see cref="IHttpContextAccessor"/> class.</param>
    /// <param name="identifier">The id representing this application deployment.</param>
    /// <param name="healthCheckRoute">The route used to check if the application is alive (to marked as synthetic).</param>
    public TelemetryInitializer(IHttpContextAccessor httpContextAccessor, string identifier, string healthCheckRoute)
        : base(httpContextAccessor)
    {
        this.identifier = identifier;
        this.healthCheckRoute = healthCheckRoute;
    }

    /// <summary>
    /// A method with the telemetry and http context where we can update what is going to be reported.
    /// </summary>
    /// <param name="platformContext">The HttpContext.</param>
    /// <param name="requestTelemetry">The RequestTelemetry.</param>
    /// <param name="telemetry">The ApplicationInsights Telemetry.</param>
    protected override void OnInitializeTelemetry(HttpContext platformContext, RequestTelemetry requestTelemetry, ITelemetry telemetry)
    {
        if (telemetry is ISupportProperties itemProperties)
        {
            itemProperties.Properties[K2IdentifierPropertyName] = identifier;
        }

        if (platformContext != null && string.IsNullOrEmpty(telemetry?.Context?.Operation?.SyntheticSource))
        {
            var path = platformContext.Request.Path;

            if (path.StartsWithSegments(healthCheckRoute, StringComparison.OrdinalIgnoreCase))
            {
                telemetry.Context.Operation.SyntheticSource = SyntheticSourceHeaderValue;
            }
        }
    }
}
