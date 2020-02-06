// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2Bridge.DAL
{
    /// <summary>
    /// </summary>
    public interface ILazySchemaRetrieverFactory
    {
        ILazySchemaRetriever Make(string indexName);
    }
}
