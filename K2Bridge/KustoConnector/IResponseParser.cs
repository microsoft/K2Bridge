// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2Bridge.KustoConnector
{
    using System;
    using System.Data;
    using K2Bridge.Models;
    using K2Bridge.Models.Response;

    /// <summary>
    /// An interface for response parsing.
    /// </summary>
    public interface IResponseParser
    {
        /// <summary>
        /// Parse kusto IDataReader response into ElasticResponse.
        /// </summary>
        /// <param name="reader">Kusto IDataReader response.</param>
        /// <param name="queryData">QueryData containing query information.</param>
        /// <param name="timeTaken">TimeSpan representing query execution duration.</param>
        /// <returns>"ElasticResponse".</returns>
        ElasticResponse Parse(IDataReader reader, QueryData queryData, TimeSpan timeTaken);
    }
}