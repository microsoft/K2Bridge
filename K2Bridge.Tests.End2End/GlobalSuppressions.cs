// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

// This file is used by Code Analysis to maintain SuppressMessage
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given
// a specific target and scoped to a namespace, type, member, etc.
using System.Diagnostics.CodeAnalysis;

[assembly: SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1614:Element parameter documentation should have text", Justification = "TEMP", Scope = "member", Target = "~M:K2Bridge.Tests.End2End.PopulateElastic.CreateIndex(K2Bridge.Tests.End2End.TestElasticClient,System.String,System.String)~System.Threading.Tasks.Task{Newtonsoft.Json.Linq.JToken}")]
[assembly: SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1614:Element parameter documentation should have text", Justification = "TEMP", Scope = "member", Target = "~M:K2Bridge.Tests.End2End.PopulateKusto.Populate(Kusto.Data.KustoConnectionStringBuilder,System.String,System.String,System.String,System.String,System.String)~System.Threading.Tasks.Task{Kusto.Ingest.IKustoIngestionResult}")]
[assembly: SuppressMessage("StyleCop.CSharp.ReadabilityRules", "SA1117:Parameters should be on same line or separate lines", Justification = "Will be fixed in a seperate PR", Scope = "member", Target = "~M:K2Bridge.Tests.End2End.PrometheusTest.PrometheusTelemetry_WhenQueryParsed_ThenExposeQueryMetrics~System.Threading.Tasks.Task")]
