// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace K2Bridge.RequestHandlers
{
    using System;
    using System.Data;
    using System.Net;
    using K2Bridge.KustoConnector;
    using K2Bridge.Models.Response.Metadata;
    using Microsoft.Extensions.Logging;
    using Newtonsoft.Json;

    /// <summary>
    /// This class handles the field capability request - available fields and their type.
    /// </summary>
    internal class FieldCapabilityRequestHandler : RequestHandlerBase
    {
        public FieldCapabilityRequestHandler(HttpListenerContext requestContext, IQueryExecutor kustoClient, string requestId, ILogger logger)
            : base(requestContext, kustoClient, requestId, logger)
        {
        }

        public static bool CanAnswer(string rawUrl)
        {
            return rawUrl.Contains("/_field_caps");
        }

        public string PrepareResponse(string rawUrl)
        {
            try
            {
                string indexName = this.IndexNameFromURL(rawUrl);
                string kustoCommand = $".show database schema | where TableName=='{indexName}' and ColumnName!='' | project ColumnName, ColumnType";
                IDataReader kustoResults = this.Kusto.ExecuteControlCommand(kustoCommand);

                var response = new FieldCapabilityResponse();

                while (kustoResults.Read())
                {
                    IDataRecord record = kustoResults;
                    var fieldCapabilityElement = FieldCapabilityElement.Create(record);
                    if (string.IsNullOrEmpty(fieldCapabilityElement.Type))
                    {
                        this.Logger.LogWarning($"Field: {fieldCapabilityElement.Name} doesn't have a type.");
                    }

                    response.AddField(fieldCapabilityElement);

                    this.Logger.LogDebug($"Found field: {fieldCapabilityElement.Name} with type: {fieldCapabilityElement.Type}");
                }

                kustoResults.Close();

                return JsonConvert.SerializeObject(response);
            }
            catch (Exception ex)
            {
                this.Logger.LogError(ex, $"Failed to retrieve indexes");
                throw;
            }
        }
    }
}