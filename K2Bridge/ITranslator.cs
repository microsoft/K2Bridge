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
        QueryData Translate(string header, string query);
    }
}