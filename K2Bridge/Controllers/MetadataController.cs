// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2Bridge.Controllers
{
    using System;
    using System.Net.Http;
    using System.Threading.Tasks;
    using K2Bridge.HttpMessages;
    using Microsoft.AspNetCore.Http;
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
                return await PassthroughInternalAsync(HttpContext);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occured when sending a passthrough request");
                throw;
            }
        }

        /// <summary>
        /// Take the original HTTP request and send it to the fallback elastic instance (passthrough).
        /// </summary>
        /// <param name="context">The original HTTP context.</param>
        /// <returns>The HTTP response delivered by the fallback elastic instance.</returns>
        internal async Task<IActionResult> PassthroughInternalAsync(HttpContext context)
        {
            // a workaround for an illegal template path. Converts the URL back to :. format
            // A Rewrite rule initiall replaces the following :. into ::, and now we convert back.
            // /_template/kibana_index_template:.kibana
            if (context.Request.Path.Value.Contains("_template", StringComparison.OrdinalIgnoreCase))
            {
                context.Request.Path = context.Request.Path.Value.Replace("::", ":.", StringComparison.OrdinalIgnoreCase);
            }

            var httpClient = clientFactory.CreateClient("elasticFallback");

            HttpRequestMessageFeature hreqmf = new HttpRequestMessageFeature(context);
            HttpRequestMessage remoteHttpRequestMessage = hreqmf.HttpRequestMessage;
            remoteHttpRequestMessage.Headers.Clear();

            // update the target host of the request
            remoteHttpRequestMessage.RequestUri =
                new Uri(httpClient.BaseAddress, remoteHttpRequestMessage.RequestUri.AbsolutePath);

            var remoteResponse = await httpClient.SendAsync(remoteHttpRequestMessage);
            context.Response.RegisterForDispose(remoteResponse);

            return new HttpResponseMessageResult(remoteResponse);
        }
    }
}