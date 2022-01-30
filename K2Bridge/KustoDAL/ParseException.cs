// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2Bridge.KustoDAL;

using System;

/// <summary>
/// Thrown when trying to visit a clause and it mandatory properties
/// are wrong/missing.
/// </summary>
public class ParseException : K2Exception
{
    /// <summary>
    /// Phase name.
    /// </summary>
    public const string ParsePhaseName = "parse";

    /// <summary>
    /// Initializes a new instance of the <see cref="ParseException"/> class.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    /// <param name="innerException">The exception that is the cause of the current exception,
    /// or a null reference(Nothing in Visual Basic) if no inner exception is specified.</param>
    public ParseException(string message, Exception innerException)
        : base(message, innerException, ParsePhaseName)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ParseException"/> class.
    /// Throws an Argument exception, the only way to create this type is with the full constructor.
    /// </summary>
    public ParseException()
        : base()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ParseException"/> class.
    /// Throws an Argument exception, the only way to create this type is with the full constructor.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    public ParseException(string message)
        : base(message)
    {
    }
}
