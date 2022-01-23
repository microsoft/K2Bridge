// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

// This file is used by Code Analysis to maintain SuppressMessage
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given
// a specific target and scoped to a namespace, type, member, etc.
using System.Diagnostics.CodeAnalysis;

[assembly: SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "TEMP", Scope = "member", Target = "~M:K2Bridge.Startup.Configure(Microsoft.AspNetCore.Builder.IApplicationBuilder,Microsoft.AspNetCore.Hosting.IWebHostEnvironment)")]
[assembly: SuppressMessage("Performance", "CA1815:Override equals and operator equals on value types", Justification = "Inherited functionality is enough", Scope = "type", Target = "~T:K2Bridge.Models.QueryData")]

[assembly: SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:Elements should be documented", Justification = "TODO: complete docs", Scope = "type", Target = "~T:K2Bridge.Startup")]
[assembly: SuppressMessage("Design", "CA1062:Validate arguments of public methods", Justification = "Cannot be null", Scope = "member", Target = "~M:K2Bridge.CorrelationIdHeaderMiddleware.Invoke(Microsoft.AspNetCore.Http.HttpContext)~System.Threading.Tasks.Task")]

[assembly: SuppressMessage("Usage", "CA2254:Template should be a static expression", Justification = "Generic ensure method", Scope = "member", Target = "~M:K2Bridge.Ensure.ConditionIsMet(System.Boolean,System.String,Microsoft.Extensions.Logging.ILogger)")]
[assembly: SuppressMessage("Usage", "CA2254:Template should be a static expression", Justification = "Generic ensure method", Scope = "member", Target = "~M:K2Bridge.Ensure.ConstructMessageAndThrowArgumentOrNullArgument``1(``0,System.String,System.String,Microsoft.Extensions.Logging.ILogger)")]
[assembly: SuppressMessage("StyleCop.CSharp.NamingRules", "SA1316:Tuple element names should use correct casing", Justification = "Kusto method doesn't have the right case.", Scope = "member", Target = "~M:K2Bridge.KustoDAL.KustoQueryExecutor.ExecuteQueryAsync(K2Bridge.Models.QueryData,K2Bridge.Models.RequestContext)~System.Threading.Tasks.Task{System.ValueTuple{System.TimeSpan,System.Data.IDataReader}}")]
[assembly: SuppressMessage("Naming", "CA1711:Identifiers should not have incorrect suffix", Justification = "Removing Collection from the name will cause other issues", Scope = "type", Target = "~T:K2Bridge.Models.Response.HitsCollection")]
