// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2Bridge.Visitors
{
    using System;

    /// <summary>
    /// Thrown when trying to visit a clause and it mandatory properties
    /// are wrong/missing.
    /// </summary>
    public class IllegalClauseException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="IllegalClauseException"/> class.
        /// </summary>
        public IllegalClauseException()
            : base("Clause is missing mandatory properties or has invalid values")
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="IllegalClauseException"/> class.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public IllegalClauseException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="IllegalClauseException"/> class.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        /// <param name="innerException">The exception that is the cause of the current exception,
        /// or a null reference(Nothing in Visual Basic) if no inner exception is specified.</param>
        public IllegalClauseException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
