// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

using Destructurama.Attributed;

namespace K2Bridge.Models;

/// <summary>
/// A wrapper class for data with potential PII data.
/// This implementation allows Data to be replaced or redacted.
/// </summary>
public class SensitiveData
{
    private const string DefaultRedactMsg = "Redacted";

    /// <summary>
    /// Initializes a new instance of the <see cref="SensitiveData"/> class.
    /// </summary>
    public SensitiveData()
    {
        RedactMessage = DefaultRedactMsg;
    }

    /// <summary>
    /// Gets or sets the message to display when data is redacted.
    /// </summary>
    [NotLogged]
    public string RedactMessage { get; set; }

    /// <summary>
    /// Gets or sets the data to be logged.
    /// </summary>
    public object Data { get; set; }
}
