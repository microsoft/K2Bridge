// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

using System.IO;
using Prometheus;
using Serilog.Core;
using Serilog.Events;

namespace K2Bridge.Telemetry;

/// <summary>
/// Logging custom sink that increments Prometheus exception counters every time
/// exceptions are logged.
///
/// Exposes a Prometheus counter called "exceptions" which counts all logged
/// exceptions, and a labeled "exceptions_by_type" counter that counts exceptions
/// grouped by type and context, e.g.:
/// exceptions_by_type{ExceptionType="JsonReaderException",SourceContext="K2Bridge.ElasticQueryTranslator",ActionName="K2Bridge.Controllers.QueryController.SearchAsync (K2Bridge)"}.
/// </summary>
public class PrometheusSerilogSink : ILogEventSink
{
    /// <summary>
    /// Internal Serilog format code to render properties without extra double quotes.
    /// </summary>
    private const string SerilogRawFormat = "l";

    /// <summary>
    /// Prometheus counter for all logged exceptions.
    /// </summary>
    private static readonly Counter ExceptionsCounter = Prometheus.Metrics
        .CreateCounter("exceptions", "Exceptions logged");

    /// <summary>
    /// Prometheus counter for all exceptions grouped by type, context and action.
    /// </summary>
    private static readonly Counter ExceptionsByTypeCounter = Prometheus.Metrics
        .CreateCounter("exceptions_by_type", "Exceptions, by type", new CounterConfiguration
        {
            LabelNames = new[] { "ExceptionType", "SourceContext", "ActionName" },
        });

    /// <summary>
    /// Emit event.
    /// </summary>
    /// <param name="logEvent">The log event.</param>
    public void Emit(LogEvent logEvent)
    {
        if (logEvent == null || logEvent.Exception == null)
        {
            return;
        }

        ExceptionsCounter.Inc();

        var typeName = logEvent.Exception.GetType().Name;
        using var sourceContext = new StringWriter();
        if (logEvent.Properties.TryGetValue("SourceContext", out var prop))
        {
            prop.Render(sourceContext, SerilogRawFormat);
        }

        using var actionName = new StringWriter();
        if (logEvent.Properties.TryGetValue("ActionName", out prop))
        {
            prop.Render(actionName, SerilogRawFormat);
        }

        ExceptionsByTypeCounter.WithLabels(
            typeName,
            sourceContext.ToString(),
            actionName.ToString())
            .Inc();
    }
}
