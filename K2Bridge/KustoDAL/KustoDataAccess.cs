﻿// Copyright (c) Microsoft Corporation. All rights reserved.
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
    using Microsoft.Extensions.Logging;
    using Newtonsoft.Json.Linq;

    /// <summary>
    /// DAL for Kusto.
    /// </summary>
    internal class KustoDataAccess : IKustoDataAccess
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="KustoDataAccess"/> class.
        /// </summary>
        /// <param name="kustoClient">Query Executor.</param>
        /// <param name="requestContext">An object that represents properties of the entire request process.</param>
        /// <param name="logger">A logger.</param>
        /// <param name="dynamicSamplePercentage">Percentage of table to sample when building a dynamic field. When empty, queries the entire table.</param>
        public KustoDataAccess(IQueryExecutor kustoClient, RequestContext requestContext, ILogger<KustoDataAccess> logger, double? dynamicSamplePercentage = null)
        {
            Kusto = kustoClient;
            RequestContext = requestContext;
            Logger = logger;
            DynamicSamplePercentage = dynamicSamplePercentage;
        }

        private IQueryExecutor Kusto { get; set; }

        private RequestContext RequestContext { get; set; }

        private ILogger Logger { get; set; }

        private double? DynamicSamplePercentage { get; }

        /// <summary>
        /// Executes a query to Kusto for Fields Caps.
        /// </summary>
        /// <param name="indexName">Index name.</param>
        /// <returns>An object with the field caps.</returns>
        public async Task<FieldCapabilityResponse> GetFieldCapsAsync(string indexName)
        {
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
                    return response;
                }

                Logger.LogDebug("Getting schema for function '{@indexName}'", indexName);
                string functionQuery = $"{tableName} | {KustoQLOperators.GetSchema} | project ColumnName, ColumnType=DataType";
                var functionQueryData = new QueryData(functionQuery, tableName, null, null);
                var (timeTaken, reader) = await Kusto.ExecuteQueryAsync(functionQueryData, RequestContext);
                await MapFieldCaps(reader, response, tableName);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error while executing GetFieldCaps.");
                throw;
            }

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
            var query = DynamicSamplePercentage.HasValue ?
                $@"{KustoQLOperators.Let} percentage = {DynamicSamplePercentage} / 100.0;
{KustoQLOperators.Let} table_count = {KustoQLOperators.ToScalar}({tableName} | {KustoQLOperators.Count});
{tableName} | {KustoQLOperators.Sample} {KustoQLOperators.ToInt}({KustoQLOperators.Floor}(table_count * percentage, 1)) | {KustoQLOperators.Summarize} {KustoQLOperators.BuildSchema}({fieldCapabilityElement.Name})" :
                $"{tableName} | {KustoQLOperators.Summarize} {KustoQLOperators.BuildSchema}({fieldCapabilityElement.Name})";
            var (_, result) = await Kusto.ExecuteQueryAsync(new QueryData(query, tableName), RequestContext);
            result.Read();
            var jsonResult = result[0];
            var stack = new Stack<(string, JObject)>();

            FieldCapabilityElement initialField;
            switch (jsonResult)
            {
                case JArray arr:
                    initialField = FieldCapabilityElementFactory.CreateFromNameAndKustoShorthandType(fieldCapabilityElement.Name, CombineValues(arr));
                    break;
                case JObject obj:
                    initialField = FieldCapabilityElementFactory.CreateFromNameAndKustoShorthandType(fieldCapabilityElement.Name, "dynamic");
                    stack.Push((fieldCapabilityElement.Name, obj));
                    break;
                case JValue v:
                    initialField = FieldCapabilityElementFactory.CreateFromNameAndKustoShorthandType(fieldCapabilityElement.Name, CombineValues(v));
                    break;
                default:
                    throw new InvalidOperationException($"Unexpected type {jsonResult.GetType()}");
            }

            response.AddField(initialField);
            Logger.LogDebug("Found dynamic field: {@initialField}", initialField);

            while (stack.Count > 0)
            {
                var (path, jsonObject) = stack.Pop();
                foreach (var property in jsonObject.Properties())
                {
                    var newPath = path + "." + property.Name;
                    if (property.Value.Type != JTokenType.Object)
                    {
                        var newField = FieldCapabilityElementFactory.CreateFromNameAndKustoShorthandType(newPath, CombineValues(property.Value));
                        response.AddField(newField);
                        Logger.LogDebug("Added dynamic subfield for {@initialField} - {@newField} ", initialField, newField);

                        continue;
                    }

                    /* The special name `indexer` indicates that the field is an array.
                     * In kibana, there is no difference between an array field and a normal field (for example, any int field can contain a single int value or an array of int values).
                     * So we test for the field, if it's null we continue parsing the object, and if it's not we parse the inner fields.
                     */
                    var indexer = property.Value["`indexer`"];
                    switch (indexer)
                    {
                        case null:
                            stack.Push((newPath, (JObject)property.Value));
                            break;
                        case JObject j:
                            stack.Push((newPath, j));
                            break;
                        default:
                            var newField = FieldCapabilityElementFactory.CreateFromNameAndKustoShorthandType(
                                newPath,
                                CombineValues(indexer));
                            response.AddField(newField);
                            Logger.LogDebug("Added dynamic array subfield for {@initialField} - {@newField} ", initialField, newField);
                            break;
                    }
                }
            }
        }

        private string CombineValues(JToken property)
        {
            return property is JArray ? "string" : property.ToString();
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