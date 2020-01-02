// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2Bridge.KustoConnector
{
    using System;
    using System.Data;
    using K2Bridge.Models.Response;

    /// <summary>
    /// An interface for response parsing.
    /// </summary>
    public interface IResponseParser
    {
        ElasticResponse ParseElasticResponse(IDataReader reader, QueryData queryData, TimeSpan timeTaken);
    }
}