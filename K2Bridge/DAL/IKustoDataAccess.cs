// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2Bridge.DAL
{
    using System.Threading.Tasks;
    using K2Bridge.Models;
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
        /// <param name="requestContext">An object that represents properties that will be sent to Kusto.</param>
        /// <returns>An object with the field caps.</returns>
        Task<FieldCapabilityResponse> GetFieldCapsAsync(string indexName, RequestContext requestContext);

        /// <summary>
        /// Executes a query to Kusto for Index List.
        /// </summary>
        /// <param name="indexName">Index name.</param>
        /// <param name="requestContext">An object that represents properties that will be sent to Kusto.</param>
        /// <returns>A list of Indexes.</returns>
        Task<IndexListResponseElement> GetIndexListAsync(string indexName, RequestContext requestContext);
    }
}
