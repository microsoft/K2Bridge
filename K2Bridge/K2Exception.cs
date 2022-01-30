// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2Bridge;

using System;

/// <summary>
/// Thrown when trying to visit a clause and it mandatory properties
/// are wrong/missing.
/// </summary>
public abstract class K2Exception : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="K2Exception"/> class.
    /// Throws an Argument exception, the only way to create this type is with the full constructor.
    /// </summary>
    public K2Exception()
        : base()
    {
        throw new ArgumentException("Initialize a K2Exception with an internal exception and phase name");
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="K2Exception"/> class.
    /// Throws an Argument exception, the only way to create this type is with the full constructor.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    public K2Exception(string message)
        : base(message)
    {
        throw new ArgumentException("Initialize a K2Exception with an internal exception and phase name");
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="K2Exception"/> class.
    /// Throws an Argument exception, the only way to create this type is with the full constructor.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    /// <param name="innerException">The exception that is the cause of the current exception,
    /// or a null reference(Nothing in Visual Basic) if no inner exception is specified.</param>
    public K2Exception(string message, Exception innerException)
        : base(message, innerException)
    {
        throw new ArgumentException("Initialize a K2PhaseException with an internal exception and phase name");
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="K2Exception"/> class.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    /// <param name="innerException">The exception that is the cause of the current exception,
    /// or a null reference(Nothing in Visual Basic) if no inner exception is specified.</param>
    /// <param name="phaseName">The logical phase where the exception originated from.</param>
    protected K2Exception(string message, Exception innerException, string phaseName)
        : base(message, innerException)
    {
        PhaseName = phaseName;
    }

    /// <summary>
    /// Gets the Phase Name.
    /// </summary>
    public string PhaseName { get; }
}
