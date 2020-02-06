// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2Bridge.Controllers
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Threading.Tasks;
    using K2Bridge.HttpMessages;
    using K2Bridge.KustoConnector;
    using K2Bridge.Models.Response;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Handles requests to Kusto.
    /// </summary>
    [Route("")]
    [ApiController]
    public class QueryController : ControllerBase
    {
        private readonly IQueryExecutor queryExecutor;
        private readonly ITranslator translator;
        private readonly ILogger<QueryController> logger;
        private readonly IResponseParser responseParser;

        /// <summary>
        /// Initializes a new instance of the <see cref="QueryController"/> class.
        /// </summary>
        /// <param name="queryExecutor">IQueryExecutor instance used to execute kusto queries.</param>
        /// <param name="translator">ITranslator instance used to translate elastic queries to kusto.</param>
        /// <param name="logger">ILogger instance used to log.</param>
        /// <param name="responseParser">IResponseParser instance used to parse kusto response.</param>
        public QueryController(
            IQueryExecutor queryExecutor,
            ITranslator translator,
            ILogger<QueryController> logger,
            IResponseParser responseParser)
        {
            this.queryExecutor = queryExecutor ?? throw new ArgumentNullException(nameof(queryExecutor));
            this.translator = translator ?? throw new ArgumentNullException(nameof(translator));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.responseParser = responseParser ?? throw new ArgumentNullException(nameof(responseParser));
        }

        /// <summary>
        /// Perform a Kibana search query against the data backend.
        /// </summary>
        /// <param name="totalHits">totalHits parameter coming from Kibana (currently not used).</param>
        /// <param name="ignoreThrottled">ignoreThrottled parameter coming from Kibana (currently not used).</param>
        /// <returns>An ElasticResponse object or a passthrough object if an error occured.</returns>
        [HttpPost(template: "_msearch")]
        [Consumes("application/json", "application/x-ndjson")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(ElasticResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(HttpResponseMessageResult), StatusCodes.Status200OK)]
        public async Task<IActionResult> SearchAsync(
            [FromQuery(Name = "rest_total_hits_as_int")] bool totalHits,
            [FromQuery(Name = "ignore_throttled")] bool ignoreThrottled)

        // Model binding does not work as the application/json message contains an application/nd-json payload
        // once Kibana sends the right ContentType the line below can be commented in.
        // [FromBody] IEnumerable<string> rawQueryData
        {
            try
            {
                // It's not common for Kibana to ask the response to be compressed.
                // Since by default we ignore this, adding a log to see if it does happen.
                if (logger.IsEnabled(LogLevel.Warning))
                {
                    var hasEncodingHeader = HttpContext.Request.Headers?.TryGetValue("accept-encoding", out var encodingData);
                    if (hasEncodingHeader ?? false)
                    {
                        logger.LogWarning("Unsupported encoding was requested: {encodingData}", encodingData);
                    }
                }

                return await SearchInternalAsync(totalHits, ignoreThrottled, await ExtractBodyAsync());
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "Failed to execute search query, returning 500.");
                return BadRequest(exception);
            }
        }

        /// <summary>
        /// Internal implementation of the search API logic.
        /// Mainly used to improve testability (as certain parameters needs to be extracted from the body).
        /// </summary>
        /// <param name="totalHits">Total Hits.</param>
        /// <param name="ignoreThrottled">Ignore Throttles.</param>
        /// <param name="rawQueryData">Body Payload.</param>
        /// <returns>An ElasticResponse object.</returns>
        internal async Task<IActionResult> SearchInternalAsync(bool totalHits, bool ignoreThrottled, string rawQueryData)
        {
            var sw = new Stopwatch();
            sw.Start();

            // Extract Query
            if (rawQueryData == null)
            {
                logger.LogError("Invalid request body. rawQueryData is null.");
                throw new ArgumentException("Invalid request payload", nameof(rawQueryData));
            }

            (string header, string query) = ControllerExtractMethods.SplitQueryBody(rawQueryData);
            if (string.IsNullOrEmpty(header) || string.IsNullOrEmpty(query))
            {
                logger.LogError("Invalid request body. header or query are empty.");
                throw new ArgumentException("Invalid arguments query or header are empty");
            }

            // Translate Query
            var translatedQuery = translator.Translate(header, query);
            logger.LogDebug($"Translated query:\n{translatedQuery.QueryCommandText}");

            // Execute Query
            var (timeTaken, dataReader) = await queryExecutor.ExecuteQueryAsync(translatedQuery);

            // Parse Response
            var elasticResponse = responseParser.Parse(dataReader, translatedQuery, timeTaken);
            sw.Stop();
            logger.LogDebug($"[metric] search request duration: {sw.Elapsed}");
            return Ok(elasticResponse);
        }

        /// <summary>
        /// Reads the body of an HttpRequest.
        /// </summary>
        /// <param name="request">Input request.</param>
        /// <returns>The body as string.</returns>
        private async Task<string> ExtractBodyAsync()
        {
            using var reader = new StreamReader(Request.Body);
            return await reader.ReadToEndAsync();
        }
    }
}