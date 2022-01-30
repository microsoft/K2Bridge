// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2Bridge.Controllers
{
    using System.Threading.Tasks;
    using K2Bridge.KustoDAL;
    using K2Bridge.Models.Response.Metadata;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Handles Index List requests.
    /// The original request produced by Kibana 7 is in the format of:
    /// GET /_resolve/index/* HTTP/1.1.
    /// </summary>
    public class IndexListController : ControllerBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="IndexListController"/> class.
        /// </summary>
        /// <param name="kustoDataAcess">An instance of <see cref="IKustoDataAccess"/>.</param>
        /// <param name="logger">An instance of <see cref="ILogger"/>.</param>
        public IndexListController(IKustoDataAccess kustoDataAcess, ILogger<IndexListController> logger)
        {
            Logger = logger;
            KustoDataAccess = kustoDataAcess;
        }

        private IKustoDataAccess KustoDataAccess { get; set; }

        private ILogger Logger { get; set; }

        /// <summary>
        /// Resolve the index.
        /// </summary>
        /// <param name="indexName">The index pattern to process.</param>
        /// <returns>The table list in the Kusto database.</returns>
        [HttpGet("_resolve/index/{indexName}")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(ResolveIndexResponse), StatusCodes.Status200OK)]
        public async Task<IActionResult> Resolve(string indexName)
        {
            var response = await KustoDataAccess.ResolveIndexAsync(indexName);

            return Ok(response);
        }
    }
}
