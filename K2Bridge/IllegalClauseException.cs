// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
namespace K2Bridge
{
    using System;

    // Thrown when trying to visit a clause and it mandatory properties
    // are wrong/missing
    public class IllegalClauseException : Exception
    {
        public IllegalClauseException()
            : base("Clause is missing mandatory properties or has invalid values")
        {
        }

        public IllegalClauseException(string message)
            : base(message)
        {
        }

        public IllegalClauseException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
