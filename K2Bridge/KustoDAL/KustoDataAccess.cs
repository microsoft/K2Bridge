// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2Bridge.KustoDAL
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Threading.Tasks;
    using K2Bridge.Factories;
    using K2Bridge.Models;
    using K2Bridge.Models.Response.Metadata;
    using K2Bridge.Utils;
    using K2Bridge.Visitors;
    using Microsoft.Extensions.Caching.Memory;
    using Microsoft.Extensions.Logging;
    using Newtonsoft.Json.Linq;

    /// <summary>
    /// DAL for Kusto.
    /// </summary>
    internal class KustoDataAccess : IKustoDataAccess
    {
        private readonly IMemoryCache cache;

        /// <summary>
        /// Initializes a new instance of the <see cref="KustoDataAccess"/> class.
        /// </summary>
        /// <param name="cache"></param>
        /// <param name="kustoClient">Query Executor.</param>
        /// <param name="requestContext">An object that represents properties of the entire request process.</param>
        /// <param name="logger">A logger.</param>
        /// <param name="maxDynamicSamples">Maximum amount of entries form the table to sample when building a dynamic field. When empty, queries the entire table.</param>
        /// <param name="maxDynamicSamplesIngestionTimeHours"></param>
        public KustoDataAccess(IMemoryCache cache, IQueryExecutor kustoClient, RequestContext requestContext, ILogger<KustoDataAccess> logger, ulong? maxDynamicSamples = null, ulong? maxDynamicSamplesIngestionTimeHours = null)
        {
            this.cache = cache;
            Kusto = kustoClient;
            RequestContext = requestContext;
            Logger = logger;
            MaxDynamicSamples = maxDynamicSamples;
            MaxDynamicSamplesIngestionTimeHours = maxDynamicSamplesIngestionTimeHours;
        }

        private IQueryExecutor Kusto { get; set; }

        private RequestContext RequestContext { get; set; }

        private ILogger Logger { get; set; }

        private ulong? MaxDynamicSamples { get; }

        private ulong? MaxDynamicSamplesIngestionTimeHours { get; }

        /// <summary>
        /// Executes a query to Kusto for Fields Caps.
        /// </summary>
        /// <param name="indexName">Index name.</param>
        /// <param name="invalidateCache">Invalidate the cache and force refresh.</param>
        /// <returns>An object with the field caps.</returns>
        public async Task<FieldCapabilityResponse> GetFieldCapsAsync(string indexName, bool invalidateCache = false)
        {
            if (!invalidateCache && cache.TryGetValue(indexName, out var cached))
            {
                return (FieldCapabilityResponse)cached;
            }

            var response = new FieldCapabilityResponse();
            try
            {
                Logger.LogDebug("Getting schema for table '{@indexName}'", indexName);
                var (databaseName, tableName) = KustoDatabaseTableNames.FromElasticIndexName(indexName, Kusto.DefaultDatabaseName);
                string kustoCommand = $".show {KustoQLOperators.Databases} {KustoQLOperators.Schema} | {KustoQLOperators.Where} TableName=='{tableName}' {KustoQLOperators.And} DatabaseName=='{databaseName}' {KustoQLOperators.And} ColumnName!='' | {KustoQLOperators.Project} ColumnName, ColumnType";

                using IDataReader kustoResults = await Kusto.ExecuteControlCommandAsync(kustoCommand, RequestContext);
                await MapFieldCaps(kustoResults, response, tableName);

                response.AddIndex(tableName);

                if (response.Fields.Count > 0)
                {
                    cache.Set(indexName, response);
                    return response;
                }

                Logger.LogDebug("Getting schema for function '{@indexName}'", indexName);
                string functionQuery = $"{tableName.QuoteKustoTable()} | {KustoQLOperators.GetSchema} | project ColumnName, ColumnType=DataType";
                var functionQueryData = new QueryData(functionQuery, tableName, null, null);
                var (timeTaken, reader) = await Kusto.ExecuteQueryAsync(functionQueryData, RequestContext);
                await MapFieldCaps(reader, response, tableName);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error while executing GetFieldCaps.");
                throw;
            }

            cache.Set(indexName, response);
            return response;
        }

        /// <summary>
        /// Executes a query to Kusto for Index List.
        /// Searches for all tables and all functions that match the index name pattern.
        /// </summary>
        /// <param name="indexName">Index name pattern, e.g. "*", "orders*", "orders".</param>
        /// <returns>A list of Indexes matching the given name pattern.</returns>
        public async Task<IEnumerable<string>> GetTablesAndFunctions(string indexName)
        {
            var tablesAndFunctions = new List<string>();
            try
            {
                Logger.LogDebug("Listing tables matching '{@indexName}'", indexName);
                var (databaseName, tableName) = KustoDatabaseTableNames.FromElasticIndexName(indexName, Kusto.DefaultDatabaseName);
                string readTablesCommand = $".show {KustoQLOperators.Databases} {KustoQLOperators.Schema} | {KustoQLOperators.Where} TableName != '' | {KustoQLOperators.Distinct} TableName, DatabaseName | {KustoQLOperators.Search} TableName: '{tableName}' | {KustoQLOperators.Search} DatabaseName: '{databaseName}' |  {KustoQLOperators.Project} strcat(DatabaseName, \"{KustoDatabaseTableNames.Separator}\", TableName)";

                using IDataReader kustoTables = await Kusto.ExecuteControlCommandAsync(readTablesCommand, RequestContext);
                if (kustoTables != null)
                {
                    while (kustoTables.Read())
                    {
                        tablesAndFunctions.Add(Convert.ToString(kustoTables[0]));
                    }
                }

                Logger.LogDebug("Listing functions matching '{@indexName}'", indexName);
                var defaultDb = Kusto.DefaultDatabaseName;
                string readFunctionsCommand = $".show {KustoQLOperators.Functions} | {KustoQLOperators.Where} Parameters == '()' | {KustoQLOperators.Distinct} Name | {KustoQLOperators.Search} Name: '{tableName}' | {KustoQLOperators.Project} strcat(\"{defaultDb}\", \"{KustoDatabaseTableNames.Separator}\", Name)";

                using IDataReader kustoFunctions = await Kusto.ExecuteControlCommandAsync(readFunctionsCommand, RequestContext);
                if (kustoFunctions != null)
                {
                    while (kustoFunctions.Read())
                    {
                        tablesAndFunctions.Add(Convert.ToString(kustoFunctions[0]));
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error while executing GetTablesAndFunctions.");
                throw;
            }

            return tablesAndFunctions;
        }

        /// <summary>
        /// Executes a query to Kusto for Resolve Index.
        /// Searches for all tables and all functions that match the index name pattern.
        /// </summary>
        /// <param name="indexName">Index name pattern, e.g. "*", "orders*", "orders".</param>
        /// <returns>A list of Indexes matching the given name pattern.</returns>
        public async Task<ResolveIndexResponse> ResolveIndexAsync(string indexName)
        {
            var response = new ResolveIndexResponse();
            try
            {
                var tablesAndFunctions = await GetTablesAndFunctions(indexName);
                MapResolveIndexList(tablesAndFunctions, response);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error while executing ResolveIndexAsync.");
                throw;
            }

            return response;
        }

        private async Task MapFieldCaps(IDataReader kustoResults, FieldCapabilityResponse response, string tableName)
        {
            while (kustoResults.Read())
            {
                var fieldCapabilityElement = FieldCapabilityElementFactory.CreateFromDataRecord(kustoResults);

                if (fieldCapabilityElement.Type == "object")
                {
                    await HandleDynamicField(response, tableName, fieldCapabilityElement);
                }
                else
                {
                    if (string.IsNullOrEmpty(fieldCapabilityElement.Type))
                    {
                        Logger.LogWarning("Field: {@fieldCapabilityElement} doesn't have a type.", fieldCapabilityElement);
                    }

                    response.AddField(fieldCapabilityElement);
                }
            }
        }

        /// <summary>
        /// Searches through the dynamic fields in kusto, parses their inner types and adds them to the response.
        /// To do this, we run a `buildschema` query to kusto, which aggregates the rows of the dynamic columns,
        /// and returns the inferred schema as a JSON object.
        /// By default we run this query on all of the rows in the table, but using the `dynamicSamplePercentage` configuration settings we will sample a percentage of the rows.
        /// </summary>
        /// <param name="response">Response containing the field caps.</param>
        /// <param name="tableName">Name of the kusto table containing the dynamic field.</param>
        /// <param name="fieldCapabilityElement">The dynamic field itself.</param>
        /// <exception cref="InvalidOperationException">When parsing the json response yields an unexpected type.</exception>
        private async Task HandleDynamicField(FieldCapabilityResponse response, string tableName, FieldCapabilityElement fieldCapabilityElement)
        {
            var sample = MaxDynamicSamples.HasValue ? $" | {KustoQLOperators.Sample} {MaxDynamicSamples.Value}" : string.Empty;
            var ingestionTime = MaxDynamicSamplesIngestionTimeHours.HasValue ? $" | {KustoQLOperators.Where} {KustoQLOperators.IngestionTime}() > {KustoQLOperators.Ago}({MaxDynamicSamplesIngestionTimeHours.Value}{KustoQLOperators.HoursMark})" : string.Empty;
            var query = $"{tableName.QuoteKustoTable()}{ingestionTime}{sample} | {KustoQLOperators.Summarize} {KustoQLOperators.BuildSchema}({fieldCapabilityElement.Name})";
            var (_, result) = await Kusto.ExecuteQueryAsync(new QueryData(query, tableName), RequestContext);
            result.Read();
            var jsonResult = result[0];
            var stack = new Stack<(string, JToken)>();
            stack.Push((fieldCapabilityElement.Name, (JToken)jsonResult));

            while (stack.Count > 0)
            {
                var (name, jsonObject) = stack.Pop();
                /* The special name `indexer` indicates that the field is an array.
                    * In kibana, there is no difference between an array field and a normal field (for example, any int field can contain a single int value or an array of int values).
                    */
                const string specialArrayKey = "`indexer`";
                switch (jsonObject)
                {
                    case JValue v:
                        AddSingleDynamicField(response, name, v);
                        break;
                    case JArray arr:
                        AddSingleDynamicField(response, name, arr);
                        break;
                    case JObject obj when obj[specialArrayKey] != null:
                        stack.Push((name, obj[specialArrayKey]));
                        break;
                    case JObject obj:
                        AddSingleDynamicField(response, name, "dynamic");
                        foreach (var property in obj.Properties())
                        {
                            stack.Push((name + "." + property.Name, property.Value));
                        }

                        break;

                    default:
                        throw new InvalidOperationException($"Unexpected type {jsonResult.GetType()}");
                }
            }
        }

        private void AddSingleDynamicField(FieldCapabilityResponse response, string name, JToken type)
        {
            var newField = FieldCapabilityElementFactory.CreateFromNameAndKustoShorthandType(name, CombineValues(type));
            Logger.LogDebug("Added dynamic field '{@NewFieldName}' '{@NewFieldType}' ", newField.Name, newField.Type);
            response.AddField(newField);
        }

        private string CombineValues(JToken property)
        {
            return property switch
            {
                JArray or { Type: JTokenType.Null } => "string",
                _ => property.ToString(),
            };
        }

        private void MapResolveIndexList(IEnumerable<string> kustoResults, ResolveIndexResponse response)
        {
            foreach (var result in kustoResults)
            {
                var index = new ResolveIndexResponseIndex() { Name = result };
                response.AddIndex(index);

                Logger.LogDebug("Found table/function: {@index}", index);
            }
        }
    }
}
