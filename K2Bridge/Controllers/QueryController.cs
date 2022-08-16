// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2Bridge.Controllers;

using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
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
using Microsoft.Extensions.Primitives;

/// <summary>
/// Handles requests for business data from Kusto.
/// </summary>
[Route("")]
[ApiController]
public class QueryController : ControllerBase
{
    private const string UnknownIndexName = "unknown";
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
    /// Perform a Kibana multi-search query against the data backend.
    /// </summary>
    /// <param name="requestContext">An object that represents properties of the entire request process.</param>
    /// <returns>An ElasticResponse object or a passthrough object if an error occured.</returns>
    [HttpPost(template: "_msearch")]
    [Consumes("application/json", "application/x-ndjson")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(ElasticResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(HttpResponseMessageResult), StatusCodes.Status200OK)]
    public async Task<IActionResult> MultiSearchAsync([FromServices] RequestContext requestContext)
    {
        try
        {
            var rawQueryData = await ExtractBodyAsync();
            Ensure.IsNotNull(rawQueryData, nameof(rawQueryData), "Invalid request body. rawQueryData is null.", logger);

            // Extract Query
            var (header, query) = ControllerExtractMethods.SplitQueryBody(rawQueryData);

            return await SearchInternalAsync(header, query, requestContext);
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Failed to execute search query, returning 500.");
            return BadRequest(exception);
        }
    }

    /// <summary>
    /// Perform a Kibana single-search query against the data backend.
    /// </summary>
    /// <param name="indexName">The index that will be queried.</param>
    /// <param name="requestContext">An object that represents properties of the entire request process.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
    [HttpPost(template: "Query/SingleSearch/{indexName}")]
    [Consumes("application/json")]
    [Produces("application/json")]
    public async Task<IActionResult> SingleSearchAsync(
        string indexName,
        [FromServices] RequestContext requestContext)
    {
        Ensure.IsNotNullOrEmpty(indexName, nameof(indexName), null, logger);
        if (!indexName.Contains(':', StringComparison.Ordinal))
        {
            return RedirectToAction("Passthrough", "MetaData");
        }

        var header = "{\"index\":\"" + indexName + "\"}";

        return await SearchInternalAsync(header, await ExtractBodyAsync(), requestContext, true);
    }

    /// <summary>
    /// Internal implementation of the search API logic.
    /// Mainly used to improve testability (as certain parameters needs to be extracted from the body).
    /// </summary>
    /// <param name="header">The header of the query request that includes the index to be queried.</param>
    /// <param name="query">The actual query that will be executed.</param>
    /// <param name="requestContext">An object that represents properties of the entire request process.</param>
    /// <param name="isSingleDocument">True if this is part of a flow which requires a single response.</param>
    /// <returns>An ElasticResponse object.</returns>
    internal async Task<IActionResult> SearchInternalAsync(
        string header,
        string query,
        RequestContext requestContext,
        bool isSingleDocument = false)
    {
        var sw = new Stopwatch();
        sw.Start();

        Ensure.IsNotNullOrEmpty(header, nameof(header), "Invalid request body. header is null or empty.", logger);
        Ensure.IsNotNullOrEmpty(query, nameof(query), "Invalid request body. query is null or empty.", logger);

        CheckEncodingHeader();

        // Translate Query
        var (translationResult, translationError) = TryFuncReturnsElasticError(
            () => translator.TranslateQuery(header, query),
            UnknownIndexName); // At this point we don't know the index name.

        if (translationError != null)
        {
            return Ok(translationError);
        }

        logger.LogDebug("Translated query:\n{@QueryCommandText}", translationResult.QueryCommandText.ToSensitiveData());

        // Execute Query
        var ((timeTaken, dataReader), queryError) = await TryAsyncFuncReturnsElasticError(
            async () => await queryExecutor.ExecuteQueryAsync(translationResult, requestContext),
            translationResult.IndexName);

        if (queryError != null)
        {
            return Ok(queryError);
        }

        // Parse Response
        var (parsingResult, parsingError) = TryFuncReturnsElasticError(
            () =>
            {
                var elasticResponse = responseParser.Parse(dataReader, translationResult, timeTaken);

                return isSingleDocument ? (object)elasticResponse.Responses.First() : elasticResponse;
            },
            translationResult.IndexName);
        if (parsingError != null)
        {
            return Ok(parsingError);
        }

        sw.Stop();
        logger.LogDebug($"[metric] search request duration: {sw.Elapsed}");

        return Ok(parsingResult);
    }

    /// <summary>
    /// Executes a function and returns the result.
    /// if the function throws an exception, returns an elastic error response.
    /// </summary>
    /// <typeparam name="TResult">The result type of the function.</typeparam>
    /// <param name="func">The function to run.</param>
    /// <param name="indexName">index name where action is running on.</param>
    /// <returns>A tuple of either the result of running the function or an elastic error response if there's an exception.</returns>
    private async Task<(TResult Result, ElasticErrorResponse ErrorResponse)> TryAsyncFuncReturnsElasticError<TResult>(
        Func<Task<TResult>> func,
        string indexName)
    {
        try
        {
            return (await func(), null /* exception */);
        }
        catch (K2Exception exception)
        {
            logger.LogError(exception.Message, exception.InnerException);
            return (default(TResult),
                new ElasticErrorResponse(exception.GetType().Name, exception.Message, exception.PhaseName).WithRootCause(
                    exception.InnerException?.GetType().Name,
                    exception.InnerException?.Message,
                    indexName));
        }
    }

    /// <summary>
    /// Executes an async function and returns the result.
    /// if the function throws an exception, returns an elastic error response.
    /// </summary>
    /// <typeparam name="TResult">The result type of the function.</typeparam>
    /// <param name="func">The async function to run.</param>
    /// <param name="indexName">index name where action is running on.</param>
    /// <returns>A tuple of either the result of running the function or an elastic error response if there's an exception.</returns>
    private (TResult Result, ElasticErrorResponse ErrorResponse) TryFuncReturnsElasticError<TResult>(
        Func<TResult> func,
        string indexName)
    {
        try
        {
            return (func(), null);
        }
        catch (K2Exception exception)
        {
            logger.LogError(exception.Message, exception.InnerException);
            return (default(TResult),
                new ElasticErrorResponse(exception.GetType().Name, exception.Message, exception.PhaseName).WithRootCause(
                    exception.InnerException?.GetType().Name,
                    exception.InnerException?.Message,
                    indexName));
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

    private void CheckEncodingHeader()
    {
        // It's not common for Kibana to ask the response to be compressed.
        // Since by default we ignore this, adding a log to see if it does happen.
        if (logger.IsEnabled(LogLevel.Warning))
        {
            StringValues encodingData = string.Empty;
            var hasEncodingHeader = HttpContext.Request.Headers?.TryGetValue("accept-encoding", out encodingData);
            if (hasEncodingHeader ?? false)
            {
                logger.LogWarning("Unsupported encoding was requested: {EncodingData}", encodingData);
            }
        }
    }
}
