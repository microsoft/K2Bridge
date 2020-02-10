// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2Bridge.DAL
{
    /// <summary>
    /// This class is used to create the <see cref="ISchemaRetriever"/>
    /// using a factory design pattern.
    /// </summary>
    public interface ISchemaRetrieverFactory
    {
        ISchemaRetriever Make(string indexName);
    }
}
