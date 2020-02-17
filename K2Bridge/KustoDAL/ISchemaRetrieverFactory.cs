// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2Bridge.KustoDAL
{
    /// <summary>
    /// This class is used to create the <see cref="ISchemaRetriever"/>
    /// using a factory design pattern.
    /// </summary>
    public interface ISchemaRetrieverFactory
    {
        /// <summary>
        /// Use this method to create the object.
        /// </summary>
        /// <param name="indexName">The index (table) get the schema for.</param>
        /// <returns>An IShemaRetriever instance.</returns>
        ISchemaRetriever Make(string indexName);
    }
}
