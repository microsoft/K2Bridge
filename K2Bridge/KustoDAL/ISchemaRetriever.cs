// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2Bridge.KustoDAL
{
    using System.Collections;
    using System.Threading.Tasks;

    /// <summary>
    /// This is used to fetch the actual schema of a given
    /// table. It is used in the visitors path, while building the
    /// generated query in order to give an accurate translation.
    /// </summary>
    public interface ISchemaRetriever
    {
        string IndexName { get; }

        Task<IDictionary> RetrieveTableSchema();
    }
}
