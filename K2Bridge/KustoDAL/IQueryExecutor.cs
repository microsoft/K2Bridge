// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2Bridge.KustoDAL
{
    using System;
    using System.Data;
    using System.Threading.Tasks;
    using K2Bridge.Models;

    public interface IQueryExecutor
    {
        string DefaultDatabaseName
        {
            get;
        }

        Task<(TimeSpan timeTaken, IDataReader reader)> ExecuteQueryAsync(QueryData query, RequestContext requestContext);

        Task<IDataReader> ExecuteControlCommandAsync(string query, RequestContext requestContext);
    }
}