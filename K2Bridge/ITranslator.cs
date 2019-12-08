// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace K2Bridge
{
    internal interface ITranslator
    {
        QueryData Translate(string header, string query);
    }
}