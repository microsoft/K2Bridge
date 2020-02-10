// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2Bridge.Visitors
{
    /// <summary>
    /// A class to verify a condition on a clause.
    /// If the condition is not met - an <see cref="IllegalClauseException"/> is thrown.
    /// </summary>
    public static class EnsureClause
    {
        /// <summary>
        /// Throws an <see cref="IllegalClauseException"/> if the clause is null.
        /// </summary>
        /// <typeparam name="T">Type of clause.</typeparam>
        /// <param name="clause">The clause that will be validated.</param>
        /// <param name="clauseName">The name which will be presented as the clause's name in the exception predefinedMessage.</param>
        /// <param name="predefinedMessage">A predefinedMessage with which the exception will be created.</param>
        public static void IsNotNull<T>(T clause, string clauseName, string predefinedMessage = null)
        {
            if (clause == null)
            {
                throw new IllegalClauseException(predefinedMessage ?? $"Inner clause {clauseName} is null");
            }
        }

        /// <summary>
        /// Throws an <see cref="IllegalClauseException"/> if the clause is null or empty.
        /// </summary>
        /// <param name="clause">The clause that will be validated.</param>
        /// <param name="clauseName">The name which will be presented as the clause's name in the exception predefinedMessage.</param>
        /// <param name="predefinedMessage">A predefinedMessage with which the exception will be created.</param>
        public static void StringIsNotNullOrEmpty(string clause, string clauseName, string predefinedMessage = null)
        {
            if (string.IsNullOrEmpty(clause))
            {
                throw new IllegalClauseException(predefinedMessage ?? $"Inner clause {clauseName} is {(clause == null ? "null" : "empty")}");
            }
        }
    }
}
