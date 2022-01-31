// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2Bridge.Models;

/// <summary>
/// Connection Details for kusto access.
/// </summary>
public interface IConnectionDetails
{
    /// <summary>
    /// Gets the kusto cluster URL.
    /// </summary>
    string ClusterUrl { get; }

    /// <summary>
    /// Gets the kusto Default Database Name.
    /// </summary>
    string DefaultDatabaseName { get; }

    /// <summary>
    /// Gets the AAD Client ID.
    /// </summary>
    string AadClientId { get; }

    /// <summary>
    /// Gets the AAD Secret.
    /// </summary>
    string AadClientSecret { get; }

    /// <summary>
    /// Gets the AAD tenant ID.
    /// </summary>
    string AadTenantId { get; }

    /// <summary>
    /// Gets a value indicating whether we need to use a Managed Identity.
    /// </summary>
    bool UseManagedIdentity { get; }
}
