// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2Bridge.Models.Request.Queries.LuceneNet
{
    using Lucene.Net.Search;

    /// <summary>
    /// Object containing both Lucene.Net query and ESQuery.
    /// </summary>
    internal abstract class LuceneQueryBase
    {
        public Query LuceneQuery { get; set; }

        public IQuery ESQuery { get; set; }
    }
}
