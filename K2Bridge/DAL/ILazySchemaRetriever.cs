// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2Bridge.DAL
{
    using System.Collections;
    using System.Threading.Tasks;

    /// <summary>
    /// </summary>
    public interface ILazySchemaRetriever
    {
        string IndexName { get; }

        Task<IDictionary> RetrieveTableSchema();
    }
}
