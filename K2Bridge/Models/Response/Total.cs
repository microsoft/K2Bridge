// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2Bridge.Models.Response
{
    using System.Collections.Generic;
    using Newtonsoft.Json;

    /// <summary>
    /// Hits response.
    /// </summary>
    public class Total
    {
        /// <summary>
        /// Gets the relation.
        /// </summary>
        [JsonProperty("relation")]
        public const string Relation = "eq";

        /// <summary>
        /// Gets the value.
        /// </summary>
        [JsonProperty("value")]
        public long Value { get; set; }
    }
}
