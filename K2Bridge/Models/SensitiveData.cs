// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2Bridge.Models
{
    using Destructurama.Attributed;

    /// <summary>
    /// A wrapper class for data with potential PII data.
    /// This implementation allows Data to be replaced or reducted.
    /// </summary>
    public class SensitiveData
    {
        private const string DefaultReductMsg = "Reducted";

        /// <summary>
        /// Initializes a new instance of the <see cref="SensitiveData"/> class.
        /// </summary>
        public SensitiveData()
        {
            ReductMessage = DefaultReductMsg;
        }

        /// <summary>
        /// Gets or sets the message to display when data is reducted.
        /// </summary>
        [NotLogged]
        public string ReductMessage { get; set; }

        /// <summary>
        /// Gets or sets the data to be logged.
        /// </summary>
        public object Data { get; set; }
    }
}
