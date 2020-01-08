// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2Bridge.Controllers
{
    using System.Threading.Tasks;
    using K2Bridge.DAL;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Logging;
    using Newtonsoft.Json;

    /// <summary>
    /// Handle Fields capability requets.
    /// This controller is intetionally not mapped with [ApiController] attribute in order to avoid registering the route during startup.
    /// Instead, we first want to rewrite the original URL, and only then map it to this controller.
    /// Please see the related Rewrite Rule. <see cref="RewriteIndexFieldsRule"/>.
    /// The original HTTP request from Kibana is in the format of:
    /// POST /kibana_sample_data_logs/_field_caps?fields=*&ignore_unavailable=true&allow_no_indices=false HTTP/1.1
    /// Where "kibana_sample_data_logs" is an index name.
    /// The original URL will be rewritten to the following format: /FieldCapability/Process/kibana_sample_data_logs.
    /// </summary>
    public class FieldCapabilityController : ControllerBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FieldCapabilityController"/> class.
        /// </summary>
        /// <param name="kustoDataAccess">An instance of <see cref="IKustoDataAccess"/>.</param>
        /// <param name="logger">An instance of <see cref="ILogger"/>.</param>
        public FieldCapabilityController(IKustoDataAccess kustoDataAccess, ILogger<FieldCapabilityController> logger)
        {
            Logger = logger;
            Kusto = kustoDataAccess;
        }

        private IKustoDataAccess Kusto { get; set; }

        private ILogger Logger { get; set; }

        /// <summary>
        /// Process the request.
        /// </summary>
        /// <param name="indexName">Index name.</param>
        /// <returns>A response from Kusto.</returns>
        [HttpPost]
        public async Task<IActionResult> Process(string indexName)
        {
            var response = Kusto.GetFieldCaps(indexName);

            return new ContentResult()
            {
                Content = JsonConvert.SerializeObject(response),
                ContentType = "application/json",
                StatusCode = (int)System.Net.HttpStatusCode.OK,
            };
        }
    }
}