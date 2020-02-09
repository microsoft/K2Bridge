// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2Bridge
{
    using System;
    using System.Linq;
    using Microsoft.AspNetCore.Http;

    /// <summary>
    /// An extenstion class to extract headers from IHeaderDictionary.
    /// </summary>
    public static class ExtendedIHeaderDictionary
    {
        /// <summary>
        /// Extract header's value from dictionary or return <see cref="defaultValue"/>.
        /// </summary>
        /// <param name="dic"><see cref="IHeaderDictionary"/> to extract the header from.</param>
        /// <param name="headerName">Header's name.</param>
        /// <param name="defaultValue">Default value in case header does not exist.</param>
        /// <returns>The string which is the value of <see cref="headerName"/> in <see cref="dic"/>.</returns>
        public static string GetHeaderOrDefault(this IHeaderDictionary dic, string headerName, string defaultValue = null)
        {
            Ensure.IsNotNull(dic, nameof(HeaderDictionary));
            Ensure.IsNotNullOrEmpty(headerName, nameof(headerName));

            return dic.TryGetValue(headerName, out var value)
                ? value.First()
                : defaultValue ?? default;
        }

        /// <summary>
        /// Extract header's value from dictionary or throw <see cref="ArgumentException"/>.
        /// </summary>
        /// <param name="dic"><see cref="IHeaderDictionary"/> to extract the header from.</param>
        /// <param name="headerName">Header's name.</param>
        /// <returns>The string which is the value of <see cref="headerName"/> in <see cref="dic"/>.</returns>
        /// <exception cref="ArgumentException">In case the header does not exist.</exception>
        public static string GetHeaderOrThrow(this IHeaderDictionary dic, string headerName)
        {
            var value = GetHeaderOrDefault(dic, headerName);

            if (value == null)
            {
                throw new ArgumentException($"No key with {headerName} exists in HeaderDictionary", nameof(headerName));
            }

            return value;
        }

        /// <summary>
        /// Extract header's value from dictionary or return .
        /// </summary>
        /// <param name="dic"><see cref="IHeaderDictionary"/> to extract the header from.</param>
        /// <returns>The string which is the value of x-correlation-id header in <see cref="dic"/>.</returns>
        public static Guid GetCorrelationIdHeaderOrGenerateNew(this IHeaderDictionary dic)
        {
            Ensure.IsNotNull(dic, nameof(HeaderDictionary));
            const string correlationIdHeader = "x-correlation-id";
            Guid guid;

            var correlationId = GetHeaderOrDefault(dic, correlationIdHeader);

            if (correlationId == null)
            {
                guid = Guid.NewGuid();
                dic.Add(correlationIdHeader, guid.ToString());
            }
            else
            {
                guid = Guid.Parse(correlationId);
            }

            return guid;
        }
    }
}