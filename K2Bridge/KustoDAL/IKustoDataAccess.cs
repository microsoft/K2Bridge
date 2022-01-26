// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2Bridge.KustoDAL
{
    using System.Threading.Tasks;
    using K2Bridge.Models.Response.Metadata;

    /// <summary>
    /// Kusto Data Access Interface.
    /// </summary>
    public interface IKustoDataAccess
    {
        /// <summary>
        /// Executes a query to Kusto for Fields Caps.
        /// </summary>
        /// <param name="indexName">Index name.</param>
        /// <param name="invalidateCache">Invalidate the cache and force refresh.</param>
        /// <returns>An object with the field caps.</returns>
        Task<FieldCapabilityResponse> GetFieldCapsAsync(string indexName, bool invalidateCache = false);

        /// <summary>
        /// Resolves index name.
        /// </summary>
        /// <param name="indexName">Index name.</param>
        /// <returns>A list of Indexes.</returns>
        Task<ResolveIndexResponse> ResolveIndexAsync(string indexName);
    }
}
