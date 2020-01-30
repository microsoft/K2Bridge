// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2Bridge.DAL
{
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
        /// <returns>An object with the field caps.</returns>
        FieldCapabilityResponse GetFieldCaps(string indexName);

        /// <summary>
        /// Executes a query to Kusto for Index List.
        /// </summary>
        /// <param name="indexName">Index name.</param>
        /// <returns>A list of Indexes.</returns>
        IndexListResponseElement GetIndexList(string indexName);
    }
}
