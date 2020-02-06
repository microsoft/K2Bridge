// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2Bridge.DAL
{
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// This class is used to create the <see cref="LazySchemaRetriever"/>
    /// using a factory design pattern.
    /// </summary>
    public class LazySchemaRetrieverFactory : ILazySchemaRetrieverFactory
    {
        private readonly IKustoDataAccess kustoDataAccess;

        /// <summary>
        /// Initializes a new instance of the <see cref="LazySchemaRetrieverFactory"/> class.
        /// </summary>
        /// <param name="logger">The logger to be used.</param>
        /// <param name="kustoDataAccess">The DAL that will be used to fetch the schema.</param>
        public LazySchemaRetrieverFactory(ILogger<LazySchemaRetriever> logger, IKustoDataAccess kustoDataAccess)
        {
            Logger = logger;
            this.kustoDataAccess = kustoDataAccess;
        }

        private ILogger<LazySchemaRetriever> Logger { get; set; }

        /// <summary>
        /// Makes the actual <see cref="ILazySchemaRetriever"/>.
        /// </summary>
        /// <param name="indexName">The index name to be used.</param>
        /// <returns>The created <see cref="ILazySchemaRetriever"/>.</returns>
        public ILazySchemaRetriever Make(string indexName)
        {
            return new LazySchemaRetriever(
                Logger,
                kustoDataAccess,
                indexName);
        }
    }
}
