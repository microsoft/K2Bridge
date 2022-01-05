// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

// This file is used by Code Analysis to maintain SuppressMessage
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given
// a specific target and scoped to a namespace, type, member, etc.
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "TEMP", Scope = "member", Target = "~M:K2Bridge.Startup.Configure(Microsoft.AspNetCore.Builder.IApplicationBuilder,Microsoft.AspNetCore.Hosting.IWebHostEnvironment)")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1056:Uri properties should not be strings", Justification = "TEMP", Scope = "member", Target = "~P:K2Bridge.Models.IConnectionDetails.ClusterUrl")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1815:Override equals and operator equals on value types", Justification = "Inherited functionality is enough", Scope = "type", Target = "~T:K2Bridge.Models.QueryData")]

[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:Elements should be documented", Justification = "TODO: complete docs", Scope = "type", Target = "~T:K2Bridge.Startup")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1062:Validate arguments of public methods", Justification = "Cannot be null", Scope = "member", Target = "~M:K2Bridge.CorrelationIdHeaderMiddleware.Invoke(Microsoft.AspNetCore.Http.HttpContext)~System.Threading.Tasks.Task")]

[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "TODO", Scope = "member", Target = "~M:K2Bridge.Controllers.MetadataController.Passthrough~System.Threading.Tasks.Task{Microsoft.AspNetCore.Mvc.IActionResult}")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "TODO", Scope = "member", Target = "~M:K2Bridge.KustoDAL.KustoResponseParser.ReadDataResponse(System.Data.IDataReader)~Kusto.Data.Data.KustoResponseDataSet")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "TODO", Scope = "member", Target = "~M:K2Bridge.KustoDAL.LuceneHighlighter.GetHighlightedValue(System.String,System.Object)~System.String")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "TODO", Scope = "member", Target = "~M:K2Bridge.KustoDAL.LuceneHighlighter.MakeValueHighlighter(Lucene.Net.QueryParsers.QueryParser,System.String,System.String,System.String)~Lucene.Net.Search.Highlight.Highlighter")]
