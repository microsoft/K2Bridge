// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace K2Bridge
{
    /// <summary>
    /// An interface for query translation
    /// </summary>
    public interface ITranslator
    {
        QueryData Translate(string header, string query);
    }
}