// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2Bridge.KustoConnector
{
    using System;
    using System.Data;
    using System.Threading.Tasks;
    using K2Bridge.Models;

    public interface IQueryExecutor
    {
        IConnectionDetails ConnectionDetails
        {
            get;
            set;
        }

        Task<(TimeSpan timeTaken, IDataReader reader)> ExecuteQueryAsync(QueryData query);

        Task<IDataReader> ExecuteControlCommandAsync(string query);
    }
}