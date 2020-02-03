// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2Bridge.Controllers
{
    using System.Threading.Tasks;
    using K2Bridge.DAL;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Handles Index List requests.
    /// The original request produced by Kibana is in the format of:
    /// POST /*/_search?ignore_unavailable=true HTTP/1.1.
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
            Kusto = kustoDataAcess;
        }

        private IKustoDataAccess Kusto { get; set; }

        private ILogger Logger { get; set; }

        /// <summary>
        /// Process the request.
        /// </summary>
        /// <param name="indexName">The index to process.</param>
        /// <returns>The table list in the Kusto database.</returns>
        [Produces("application/json")]
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        public async Task<IActionResult> Process(string indexName)
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {
            var response = await Kusto.GetIndexListAsync(indexName);

            return Ok(response);
        }
    }
}
