// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2Bridge
{
    using System;
    using System.Collections;

    /// <summary>
    /// A class to verify a condition on a parameter.
    /// If the condition is not met - an exception is thrown.
    /// </summary>
    public static class Ensure
    {
        /// <summary>
        /// Throws an <see cref="ArgumentNullException"/> if the argument is null.
        /// </summary>
        /// <typeparam name="T">Type of argument.</typeparam>
        /// <param name="arg">The argument that will be validated.</param>
        /// <param name="argName">The name which will be presented as the argument's name in the exception message.</param>
        /// <param name="predefinedMessage">A message with which the exception will be created.</param>
        public static void IsNotNull<T>(T arg, string argName, string predefinedMessage = null)
        {
            if (arg == null)
            {
                throw new ArgumentNullException(predefinedMessage ?? $"Argument {argName} cannot be null");
            }
        }

        /// <summary>
        /// Throws an <see cref="ArgumentNullException"/> if the argument is null.
        /// </summary>
        /// <param name="arg">The argument that will be validated.</param>
        /// <param name="argName">The name which will be presented as the argument's name in the exception message.</param>
        /// <param name="predefinedMessage">A message with which the exception will be created.</param>
        public static void IsNotNullOrEmpty(ICollection arg, string argName, string predefinedMessage = null)
        {
            if (arg == null || arg.Count == 0)
            {
                ConstructMessageAndThrowArgumentOrNullArgument(arg, argName, predefinedMessage);
            }
        }

        /// <summary>
        /// Throws an <see cref="ArgumentNullException"/> if the string is null or a <see cref="ArgumentException"/> if the string is empty.
        /// </summary>
        /// <param name="arg">The argument that will be validated.</param>
        /// <param name="argName">The name which will be presented as the argument's name in the exception message.</param>
        /// <param name="predefinedMessage">A message with which the exception will be created.</param>
        public static void IsNotNullOrEmpty(string arg, string argName, string predefinedMessage = null)
        {
            if (string.IsNullOrEmpty(arg))
            {
                ConstructMessageAndThrowArgumentOrNullArgument(arg, argName, predefinedMessage);
            }
        }

        /// <summary>
        /// Throws an <see cref="ArgumentException"/> or <see cref="ArgumentNullException"/>.
        /// </summary>
        /// <typeparam name="T">Type of argument.</typeparam>
        /// <param name="arg">The argument that will be validated.</param>
        /// <param name="argName">The name which will be presented as the argument's name in the exception message.</param>
        /// <param name="predefinedMessage">A message with which the exception will be created.</param>
        private static void ConstructMessageAndThrowArgumentOrNullArgument<T>(T arg, string argName, string predefinedMessage = null)
        {
            var message = predefinedMessage ?? $"{argName} cannot be {(arg == null ? "null" : "empty")}";
            throw (arg == null)
                ? new ArgumentNullException(argName, message)
                : new ArgumentException(argName, message);
        }
    }
}
