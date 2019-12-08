// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace K2Bridge.KustoConnector
{
    using System.Data;
    using K2Bridge.Models.Response;

    public interface IQueryExecutor
    {
        ElasticResponse ExecuteQuery(QueryData query);

        IDataReader ExecuteControlCommand(string query);
    }
}