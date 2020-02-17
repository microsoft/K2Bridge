// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2Bridge.Telemetry
{
    using K2Bridge.Models;

    /// <summary>
    /// Extension method for handling pii logs.
    /// </summary>
    public static class SensitiveDataExtensions
    {
        /// <summary>
        /// Wraps data with Pii entry object.
        /// </summary>
        /// <param name="data">Data.</param>
        /// <returns>PiiLogEntry.</returns>
        public static SensitiveData ToSensitiveData(this object data)
        {
            return new SensitiveData { Data = data };
        }
    }
}
