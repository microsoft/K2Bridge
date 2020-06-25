// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2Bridge
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// A class to verify a condition on a parameter.
    /// If the condition is not met - an exception is thrown.
    /// </summary>
    public static class Ensure
    {
        /// <summary>
        /// Throws <see cref="ArgumentException"/> if the condition is not met.
        /// </summary>
        /// <param name="condition">A condition to be met.</param>
        /// <param name="conditionDescription">A description to be traced with the exception.</param>
        /// <param name="logger">Optional logger to log Error.</param>
        public static void ConditionIsMet(bool condition, string conditionDescription, ILogger logger = null)
        {
            if (!condition)
            {
                logger?.LogError(conditionDescription);
                throw new ArgumentException(conditionDescription);
            }
        }

        /// <summary>
        /// Throws an <see cref="ArgumentNullException"/> if the argument is null.
        /// </summary>
        /// <typeparam name="T">Type of argument.</typeparam>
        /// <param name="arg">The argument that will be validated.</param>
        /// <param name="argName">The name which will be presented as the argument's name in the exception message.</param>
        /// <param name="predefinedMessage">A message with which the exception will be created.</param>
        /// <param name="logger">Optional logger to log Error.</param>
        public static void IsNotNull<T>([ValidatedNotNull] T arg, string argName, string predefinedMessage = null, ILogger logger = null)
        {
            if (arg == null)
            {
                ConstructMessageAndThrowArgumentOrNullArgument(arg, argName, predefinedMessage, logger);
            }
        }

        /// <summary>
        /// Throws an <see cref="ArgumentNullException"/> if the argument is null or empty.
        /// </summary>
        /// <typeparam name="T">Type of values in argument.</typeparam>
        /// <param name="arg">The argument that will be validated.</param>
        /// <param name="argName">The name which will be presented as the argument's name in the exception message.</param>
        /// <param name="predefinedMessage">A message with which the exception will be created.</param>
        /// <param name="logger">Optional logger to log Error.</param>
        public static void IsNotNullOrEmpty<T>([ValidatedNotNull] IEnumerable<T> arg, string argName, string predefinedMessage = null, ILogger logger = null)
        {
            if (arg == null || !arg.Any())
            {
                ConstructMessageAndThrowArgumentOrNullArgument(arg, argName, predefinedMessage, logger);
            }
        }

        /// <summary>
        /// Throws an <see cref="ArgumentNullException"/> if the string is null or a <see cref="ArgumentException"/> if the string is empty.
        /// </summary>
        /// <param name="arg">The argument that will be validated.</param>
        /// <param name="argName">The name which will be presented as the argument's name in the exception message.</param>
        /// <param name="predefinedMessage">A message with which the exception will be created.</param>
        /// <param name="logger">Optional logger to log Error.</param>
        public static void IsNotNullOrEmpty(string arg, string argName, string predefinedMessage = null, ILogger logger = null)
        {
            if (string.IsNullOrEmpty(arg))
            {
                ConstructMessageAndThrowArgumentOrNullArgument(arg, argName, predefinedMessage, logger);
            }
        }

        /// <summary>
        /// Throws an <see cref="ArgumentException"/> or <see cref="ArgumentNullException"/>.
        /// </summary>
        /// <typeparam name="T">Type of argument.</typeparam>
        /// <param name="arg">The argument that will be validated.</param>
        /// <param name="argName">The name which will be presented as the argument's name in the exception message.</param>
        /// <param name="predefinedMessage">A message with which the exception will be created.</param>
        /// <param name="logger">Optional logger to log Error.</param>
        private static void ConstructMessageAndThrowArgumentOrNullArgument<T>(T arg, string argName, string predefinedMessage = null, ILogger logger = null)
        {
            var message = predefinedMessage ?? $"{argName} cannot be {(arg == null ? "null" : "empty")}";
            logger?.LogError(message);
            throw (arg == null)
                ? new ArgumentNullException(argName, message)
                : new ArgumentException(message, argName);
        }

        /// <summary>
        /// A class to note that a parameter goes through null validation.
        /// </summary>
        [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = false)]
        internal sealed class ValidatedNotNullAttribute : Attribute
        {
        }
    }
}
