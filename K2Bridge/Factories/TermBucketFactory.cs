// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2Bridge.Factories
{
    using System;
    using K2Bridge.Models.Response;

    /// <summary>
    /// Factory class for <see cref="TermBucket"/>.
    /// </summary>
    public static class TermBucketFactory
    {
        /// <summary>
        /// Creates a <see cref="TermBucket"/>.
        /// </summary>
        /// <param name="record">A record.</param>
        /// <returns>TermBucket.</returns>
        public static TermBucket CreateFromDataRecord(System.Data.IDataRecord record)
        {
            Ensure.IsNotNull(record, nameof(record));

            var key = record[(int)TermBucketDataReaderMapping.Key];

            return new TermBucket
            {
                Key = Convert.ToString(key),
            };
        }

        /// <summary>
        /// Creates a <see cref="TermBucket"/>.
        /// </summary>
        /// <param name="key">A key.</param>
        /// <returns>TermBucket.</returns>
        public static TermBucket CreateFromKey(string key)
        {
            Ensure.IsNotNull(key, nameof(key));

            return new TermBucket
            {
                Key = key,
            };
        }
    }
}
