// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2Bridge.Controllers
{
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;
    using K2Bridge.DAL;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Handles Index List requests.
    /// The original request produced by Kibana is in the format of:
    /// POST /*/_search?ignore_unavailable=true HTTP/1.1.
    /// </summary>
    [Route("/*/")]
    [ApiController]
    internal class IndexListController : ControllerBase
    {
        private readonly Regex indexNamePattern;

        /// <summary>
        /// Initializes a new instance of the <see cref="IndexListController"/> class.
        /// </summary>
        /// <param name="kustoDataAcess">An instance of <see cref="IKustoDataAccess"/>.</param>
        /// <param name="logger">An instance of <see cref="ILogger"/>.</param>
        public IndexListController(IKustoDataAccess kustoDataAcess, ILogger<IndexListController> logger)
        {
            Logger = logger;
            Kusto = kustoDataAcess;
            indexNamePattern = new Regex("/(.*?)/.*?");
        }

        private IKustoDataAccess Kusto { get; set; }

        private ILogger Logger { get; set; }

        /// <summary>
        /// Process the request.
        /// </summary>
        /// <returns>The table list in the Kusto database.</returns>
        [HttpPost(template: "_search")]
        [Produces("application/json")]
        public async Task<IActionResult> Process()
        {
            string indexName = IndexNameFromURL(HttpContext.Request.Path.ToString());
            var response = Kusto.GetIndexList(indexName);

            return Ok(response);
        }

        /// <summary>
        /// Extract index name from URL.
        /// </summary>
        /// <param name="rawUrl">Url to inspect.</param>
        /// <returns>An index name, or null if not found.</returns>
        private string IndexNameFromURL(string rawUrl)
        {
            var match = indexNamePattern.Match(rawUrl);
            if (match.Success)
            {
                // In a Regex, Groups[0] is always the entire phrase capture,
                // and the rest, are index based on the regex.
                // In our case (*.?) is the group we want to extract from regex,
                // And it is places in index[1]
                return match.Groups[1].Value;
            }

            return null;
        }
    }
}
