// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2Bridge.KustoDAL
{
    using System;
    using System.Collections;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Logging;

    /// <inheritdoc/>
    public class SchemaRetriever : ISchemaRetriever
    {
        private readonly IKustoDataAccess kustoDataAccess;
        private readonly Lazy<Task<IDictionary>> schema;

        /// <summary>
        /// Initializes a new instance of the <see cref="SchemaRetriever"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="kustoDataAccess">The Kusto DAL that will be used to actually fetch the schema.</param>
        /// <param name="indexName">The index (table name) that its schema we need.</param>
        public SchemaRetriever(ILogger<SchemaRetriever> logger, IKustoDataAccess kustoDataAccess, string indexName)
        {
            Logger = logger;
            IndexName = indexName;
            this.kustoDataAccess = kustoDataAccess;
            schema = new Lazy<Task<IDictionary>>(async () => { return await MakeDictionary(); });
        }

        /// <inheritdoc/>
        public string IndexName { get; private set; }

        private ILogger Logger { get; set; }

        /// <inheritdoc/>
        public async Task<IDictionary> RetrieveTableSchema()
        {
            return await schema.Value;
        }

        /// <summary>
        /// Gets the schema using the given DAL and returns it in a dictionary.
        /// </summary>
        /// <returns>The schema in a dictionary data structure.</returns>
        private async Task<IDictionary> MakeDictionary()
        {
            Logger.LogDebug("Retrieving table schema for {IndexName} from the datasource", IndexName);
            var response = await kustoDataAccess.GetFieldCapsAsync(IndexName);

            if (response == null)
            {
                var msg = $"Failed getting table schema for {IndexName}";
                Logger.LogError(msg);
                throw new QueryException(msg);
            }

            return response.Fields.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.Type);
        }
    }
}
