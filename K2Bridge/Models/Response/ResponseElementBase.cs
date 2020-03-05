// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2Bridge.Models.Response
{
    using Newtonsoft.Json;

    /// <summary>
    /// Base class for response element.
    /// </summary>
    public abstract class ResponseElementBase
    {
        private const int STATUS = 200;

        /// <summary>
        /// Gets or sets query execution time.
        /// </summary>
        [JsonProperty("took")]
        public int TookMilliseconds { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the query had timed out.
        /// </summary>
        [JsonProperty("timed_out")]
        public bool TimedOut { get; set; }

        /// <summary>
        /// Gets or sets the shards value.
        /// </summary>
        [JsonProperty("_shards")]
        public Shards Shards { get; set; } = new Shards();

        /// <summary>
        /// Gets or sets response hits.
        /// </summary>
        [JsonProperty("hits")]
        public HitsCollection Hits { get; set; } = new HitsCollection();

        /// <summary>
        /// Gets or sets response status.
        /// </summary>
        [JsonProperty("status")]
        public int Status { get; set; } = STATUS;

        /// <summary>
        /// Gets or sets the executed backend query.
        /// </summary>
        [JsonProperty("_backendQuery", NullValueHandling = NullValueHandling.Ignore)]
        public string BackendQuery { get; set; }
    }
}
