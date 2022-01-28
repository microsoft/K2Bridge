// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2Bridge.KustoDAL
{
    using Microsoft.Extensions.Logging;

    /// <inheritdoc/>
    public class SchemaRetrieverFactory : ISchemaRetrieverFactory
    {
        private readonly IKustoDataAccess kustoDataAccess;

        /// <summary>
        /// Initializes a new instance of the <see cref="SchemaRetrieverFactory"/> class.
        /// </summary>
        /// <param name="logger">The logger to be used.</param>
        /// <param name="kustoDataAccess">The DAL that will be used to fetch the schema.</param>
        public SchemaRetrieverFactory(ILogger<SchemaRetriever> logger, IKustoDataAccess kustoDataAccess)
        {
            Logger = logger;
            this.kustoDataAccess = kustoDataAccess;
        }

        private ILogger<SchemaRetriever> Logger { get; set; }

        /// <summary>
        /// Makes the actual <see cref="ISchemaRetriever"/>.
        /// </summary>
        /// <param name="indexName">The index name to be used.</param>
        /// <returns>The created <see cref="ISchemaRetriever"/>.</returns>
        public ISchemaRetriever Make(string indexName)
        {
            return new SchemaRetriever(
                Logger,
                kustoDataAccess,
                indexName);
        }
    }
}
