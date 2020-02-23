// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2Bridge.Models.Response.ElasticError
{
    using System.Collections.Generic;
    using Newtonsoft.Json;

   /// <summary>
    /// Error Element class.
    /// Note this implementation requires further refactoring by adding the failed_shards section.
    /// This would be where stack trace information could be provided to the user.
    /// https://dev.azure.com/csedevil/K2-bridge-internal/_workitems/edit/1761
    /// </summary>
    public class ErrorElement
    {
        private readonly List<RootCause> rootCauses = new List<RootCause>();

        /// <summary>
        /// Initializes a new instance of the <see cref="ErrorElement"/> class.
        /// </summary>
        /// <param name="type">General error type.</param>
        /// <param name="reason">General reason for error.</param>
        /// <param name="phase">Phase when error happend.</param>
        public ErrorElement(string type, string reason, string phase)
        {
            Type = type;
            Reason = reason;
            Phase = phase;
        }

        /// <summary>
        /// Gets the Root Cause Enumerable.
        /// </summary>
        [JsonProperty("root_cause")]
        public IEnumerable<RootCause> RootCauses
        {
            get
            {
                return rootCauses;
            }
        }

        /// <summary>
        /// Gets the type of error.
        /// </summary>
        public string Type { get; }

        /// <summary>
        /// Gets the reason reason for error.
        /// </summary>
        public string Reason { get; }

        /// <summary>
        /// Gets the phase when error happend.
        /// </summary>
        public string Phase { get; }

        /// <summary>
        /// Adds a root cause element to the collection.
        /// </summary>
        /// <param name="rootCause">The root cause element to add.</param>
        internal void AddRootCause(RootCause rootCause) =>
            rootCauses.Add(rootCause);
    }
}
