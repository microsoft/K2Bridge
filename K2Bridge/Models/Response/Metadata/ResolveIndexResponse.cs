// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2Bridge.Models.Response.Metadata
{
    using System.Collections.Generic;
    using Newtonsoft.Json;

    /// <summary>
    /// Resolve Index response.
    /// </summary>
    public class ResolveIndexResponse
    {
        private readonly List<ResolveIndexResponseIndex> indices = new List<ResolveIndexResponseIndex>();

        /// <summary>
        /// Gets indices.
        /// </summary>
        [JsonProperty("indices")]
        public IEnumerable<ResolveIndexResponseIndex> Indices
        {
            get { return indices; }
        }

        /// <summary>
        /// Add index to indices respons.
        /// </summary>
        /// <param name="index">Index object.</param>
        public void AddIndex(ResolveIndexResponseIndex index)
        {
            indices.Add(index);
        }
    }
}
