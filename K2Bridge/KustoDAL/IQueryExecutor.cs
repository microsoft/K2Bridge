// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2Bridge.KustoDAL;

using System;
using System.Data;
using System.Threading.Tasks;
using K2Bridge.Models;

/// <summary>
/// An interface representing a query executor class.
/// </summary>
public interface IQueryExecutor
{
    /// <summary>
    /// Gets default database name.
    /// </summary>
    string DefaultDatabaseName { get; }

    /// <summary>
    /// Execute query asynchronously.
    /// </summary>
    /// <param name="query">String respresenting a query to execute.</param>
    /// <param name="requestContext">The request context.</param>
    /// <param name="databaseName">Optional name of database</param>
    /// <returns>Task.<(TimeSpan timeTaken, IDataReader reader)> - timeTaken is the query execution time and reader is the query result.</returns>
    Task<(TimeSpan TimeTaken, IDataReader Reader)> ExecuteQueryAsync(QueryData query, RequestContext requestContext, string databaseName = "");

    /// <summary>
    /// Execute an asynchronous control command.
    /// </summary>
    /// <param name="query">The control command to execute.</param>
    /// <param name="requestContext">The request context.</param>
    /// <param name="databaseName">Optional name of database</param>
    /// <returns>Task.<IDataReader> where IDataReader is the query result.</returns>
    Task<IDataReader> ExecuteControlCommandAsync(string query, RequestContext requestContext, string databaseName = "");
}
