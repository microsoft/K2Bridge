// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2Bridge.Controllers
{
    using System;
    using System.Data;
    using System.Diagnostics;
    using System.IO;
    using System.Threading.Tasks;
    using K2Bridge.HttpMessages;
    using K2Bridge.KustoDAL;
    using K2Bridge.Models;
    using K2Bridge.Models.Response;
    using K2Bridge.Models.Response.ElasticError;
    using K2Bridge.Telemetry;
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
        public const string UnknownIndexName = "unknown";
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
        /// <param name="requestContext">An object that represents properties of the entire request process.</param>
        /// <returns>An ElasticResponse object or a passthrough object if an error occured.</returns>
        [HttpPost(template: "_msearch")]
        [Consumes("application/json", "application/x-ndjson")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(ElasticResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(HttpResponseMessageResult), StatusCodes.Status200OK)]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "An uncommon behavior.")]
        public async Task<IActionResult> SearchAsync(
            [FromQuery(Name = "rest_total_hits_as_int")] bool totalHits,
            [FromQuery(Name = "ignore_throttled")] bool ignoreThrottled,
            [FromServices] RequestContext requestContext)

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

                return await SearchInternalAsync(totalHits, ignoreThrottled, await ExtractBodyAsync(), requestContext);
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
        /// <param name="totalHits">totalHits parameter coming from Kibana (currently not used).</param>
        /// <param name="ignoreThrottled">ignoreThrottled parameter coming from Kibana (currently not used).</param>
        /// <param name="rawQueryData">Body Payload.</param>
        /// <param name="requestContext">An object that represents properties of the entire request process.</param>
        /// <returns>An ElasticResponse object.</returns>
        internal async Task<IActionResult> SearchInternalAsync(
            bool totalHits,
            bool ignoreThrottled,
            string rawQueryData,
            RequestContext requestContext)
        {
            var sw = new Stopwatch();
            sw.Start();

            Ensure.IsNotNull(rawQueryData, nameof(rawQueryData), "Invalid request body. rawQueryData is null.", logger);

            // Extract Query
            (string header, string query) = ControllerExtractMethods.SplitQueryBody(rawQueryData);

            Ensure.IsNotNullOrEmpty(header, nameof(header), "Invalid request body. header is null or empty.", logger);
            Ensure.IsNotNullOrEmpty(query, nameof(query), "Invalid request body. query is null or empty.", logger);

            // Translate Query
            (QueryData translatedQuery, bool error, ElasticErrorResponse errorResponse) translateResponse = TryFuncReturnsElasticError(
                () =>
                {
                    return translator.Translate(header, query);
                },
                UnknownIndexName); // At this point we don't know the index name.
            if (translateResponse.error)
            {
                return Ok(translateResponse.errorResponse);
            }

            logger.LogDebug("Translated query:\n{@QueryCommandText}", translateResponse.translatedQuery.QueryCommandText.ToSensitiveData());

            // Execute Query
            ((TimeSpan timeTaken, IDataReader dataReader) response, bool error, ElasticErrorResponse errorResponse) queryResponse = await TryAsyncFuncReturnsElasticError(
                async () =>
                {
                    return await queryExecutor.ExecuteQueryAsync(translateResponse.translatedQuery, requestContext);
                },
                translateResponse.translatedQuery.IndexName);
            if (queryResponse.error)
            {
                return Ok(queryResponse.errorResponse);
            }

            // Parse Response
            (ElasticResponse elasticResponse, bool error, ElasticErrorResponse errorResponse) parseResponse = TryFuncReturnsElasticError(
                () =>
                {
                    return responseParser.Parse(queryResponse.response.dataReader, translateResponse.translatedQuery, queryResponse.response.timeTaken);
                },
                translateResponse.translatedQuery.IndexName);
            if (parseResponse.error)
            {
                return Ok(parseResponse.errorResponse);
            }

            sw.Stop();
            logger.LogDebug($"[metric] search request duration: {sw.Elapsed}");
            return Ok(parseResponse.elasticResponse);
        }

        /// <summary>
        /// Executes a function and returns the result.
        /// if the function throws an exception, returns an elastic error response.
        /// </summary>
        /// <typeparam name="TResult">The result type of the function.</typeparam>
        /// <param name="func">The function to run.</param>
        /// <param name="indexName">index name where action is running on.</param>
        /// <returns>A tuple of either the result of running the function and error boolean is false, or the elastic response when the error boolean is true.</returns>
        private async Task<(TResult result, bool error, ElasticErrorResponse errorResponse)> TryAsyncFuncReturnsElasticError<TResult>(
            Func<Task<TResult>> func,
            string indexName)
        {
            try
            {
                return (await func(), false /* error */, null /* exception */);
            }
            catch (K2Exception exception)
            {
                logger.LogError(exception.Message, exception.InnerException);
                return (default(TResult), true, new ElasticErrorResponse(exception.GetType().Name, exception.Message, exception.PhaseName).
                    WithRootCause(exception.InnerException.GetType().Name, exception.InnerException.Message, indexName));
            }
        }

        /// <summary>
        /// Executes an async function and returns the result.
        /// if the function throws an exception, returns an elastic error response.
        /// </summary>
        /// <typeparam name="TResult">The result type of the function.</typeparam>
        /// <param name="func">The async function to run.</param>
        /// <param name="indexName">index name where action is running on.</param>
        /// <returns>A tuple of either the result of running the function and error boolean is false, or the elastic response when the error boolean is true.</returns>
        private (TResult result, bool error, ElasticErrorResponse errorResponse) TryFuncReturnsElasticError<TResult>(
            Func<TResult> func,
            string indexName)
        {
            try
            {
                return (func(), false, null);
            }
            catch (K2Exception exception)
            {
                logger.LogError(exception.Message, exception.InnerException);
                return (default(TResult), true, new ElasticErrorResponse(exception.GetType().Name, exception.Message, exception.PhaseName).
                    WithRootCause(exception.InnerException.GetType().Name, exception.InnerException.Message, indexName));
            }
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
