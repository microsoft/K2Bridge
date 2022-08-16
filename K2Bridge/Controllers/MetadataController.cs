// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2Bridge.Controllers;

using System;
using System.Net.Http;
using System.Threading.Tasks;
using K2Bridge.HttpMessages;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.WebApiCompatShim;
using Microsoft.Extensions.Logging;

/// <summary>
/// Handles the request that goes directly to the underlying elasticsearch
/// instance that handles all metadata requests.
/// </summary>
[Route("/Metadata")]
[ApiController]
public class MetadataController : ControllerBase
{
    /// <summary>
    /// Elasticsearch metadata instance client name.
    /// </summary>
    internal const string ElasticMetadataClientName = "elasticMetadata";
    private readonly IHttpClientFactory clientFactory;
    private readonly ILogger<MetadataController> logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="MetadataController"/> class.
    /// </summary>
    /// <param name="clientFactory">An instance of <see cref="IHttpClientFactory"/>.</param>
    /// <param name="logger">An instance of <see cref="ILogger"/>.</param>
    public MetadataController(IHttpClientFactory clientFactory, ILogger<MetadataController> logger)
    {
        this.clientFactory = clientFactory ?? throw new ArgumentNullException(nameof(clientFactory));
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Handle metadata requests to the elasticsearch.
    /// </summary>
    /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
    [HttpPost]
    [HttpHead]
    [HttpPut]
    [HttpPatch]
    [HttpOptions]
    [HttpGet]
    [HttpDelete]
    public async Task<IActionResult> Passthrough()
    {
        try
        {
            logger.LogDebug("Received request to {Method} {RequestPath} {QueryString}", HttpContext.Request.Method, HttpContext.Request.Path, HttpContext.Request.QueryString);
            return await PassthroughInternal();
        }
        catch (Exception exception)
        {
            return BadRequest(exception);
        }
    }

    /// <summary>
    /// Forwards an http message to the metadata client.
    /// </summary>
    /// <param name="clientFactory">HTTP client factory that will be used to initialize an http client.</param>
    /// <param name="message">The original HTTP message.</param>
    /// <returns>HTTP response.</returns>
    internal static async Task<HttpResponseMessage> ForwardMessageToMetadataClient(IHttpClientFactory clientFactory, HttpRequestMessage message)
    {
        var httpClient = clientFactory.CreateClient(ElasticMetadataClientName);

        // update the target host of the request
        var builder = new UriBuilder(message.RequestUri)
        {
            Scheme = httpClient.BaseAddress.Scheme,
            Host = httpClient.BaseAddress.Host,
            Port = httpClient.BaseAddress.Port,
        };
        message.RequestUri = builder.Uri;

        message.Headers.Clear();
        return await httpClient.SendAsync(message);
    }

    /// <summary>
    /// Internal implementation of the pass through API.
    /// </summary>
    /// <returns>Http response from the metadata client.</returns>
    internal async Task<IActionResult> PassthroughInternal()
    {
        HttpContext.Request.Path = ControllerExtractMethods.ReplaceBackTemplateString(HttpContext.Request.Path.Value);
        var remoteResponse = await ForwardMessageToMetadataClient(
        clientFactory,
        new HttpRequestMessageFeature(HttpContext).HttpRequestMessage);
        HttpContext.Response.RegisterForDispose(remoteResponse);
        return new HttpResponseMessageResult(remoteResponse);
    }
}
