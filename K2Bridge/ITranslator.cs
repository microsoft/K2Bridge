// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2Bridge
{
    using K2Bridge.Models;

    /// <summary>
    /// An interface for query translation.
    /// </summary>
    public interface ITranslator
    {
        /// <summary>
        /// Translate Data query.
        /// </summary>
        /// <param name="header">Query header.</param>
        /// <param name="query">Query to translate.</param>
        /// <returns>Translated QueryData.</returns>
        QueryData TranslateData(string header, string query);

        /// <summary>
        /// Translate Single Document query.
        /// </summary>
        /// <param name="header">Query header.</param>
        /// <param name="query">Query to translate.</param>
        /// <returns>Translated QueryData.</returns>
        QueryData TranslateSingleDocument(string header, string query);
    }
}