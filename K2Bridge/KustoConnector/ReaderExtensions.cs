// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2Bridge.KustoConnector
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using K2Bridge.Models.Response;

    /// <summary>
    /// Provides extension methods for kusto response objects.
    /// </summary>
    internal static class ReaderExtensions
    {
        private static readonly Random RandomId = new Random();

        private static readonly Dictionary<Type, Func<IDataRecord, int, object>> ReaderSwitch = new Dictionary<Type, Func<IDataRecord, int, object>>
        {
            { typeof(bool), (reader, index) => reader.ReadValueOrDbNull(index, () => reader.GetBoolean(index)) },
            { typeof(sbyte), (reader, index) => reader.ReadValueOrDbNull(index, () => (sbyte)reader.GetValue(index) == 0 ? false : true) },
            { typeof(short), (reader, index) => reader.ReadValueOrDbNull(index, () => reader.GetInt16(index)) },
            { typeof(int), (reader, index) => reader.ReadValueOrDbNull(index, () => reader.GetInt32(index)) },
            { typeof(long), (reader, index) => reader.ReadValueOrDbNull(index, () => reader.GetInt64(index)) },
            { typeof(double), (reader, index) => reader.ReadValueOrDbNull(index, () => reader.GetDouble(index)) },
            { typeof(string), (reader, index) => reader.ReadValueOrDbNull(index, () => reader.GetString(index)) },
            { typeof(DateTime), (reader, index) => reader.ReadValueOrDbNull(index, () => reader.GetDateTime(index)) },
            { typeof(decimal), (reader, index) => reader.ReadValueOrDbNull(index, () => reader.GetDecimal(index)) },
            { typeof(object), (reader, index) => reader.ReadValueOrDbNull(index, () => reader.GetValue(index)) },
        };

        internal static IEnumerable<Hit> ReadHits(this IDataReader reader, QueryData query)
        {
            while (reader.Read())
            {
                var hit = Hit.Create((IDataRecord)reader, query);
                hit.Id = RandomId.Next().ToString();
                yield return hit;
            }
        }

        internal static object ReadValue(this IDataRecord record, int index) =>
            ReaderSwitch.GetDictionaryValueOrDefault(
                                record.GetFieldType(index),
                                typeof(object))(record, index);

        internal static object ReadValueOrDbNull(
            this IDataRecord record,
            int index,
            Func<object> readFunc) =>
            record.IsDBNull(index) ? (object)null : readFunc();

        private static TVal GetDictionaryValueOrDefault<TKey, TVal>(this Dictionary<TKey, TVal> dictionary, TKey key, TKey defaultKey)
        {
            if (!dictionary.ContainsKey(defaultKey))
            {
                throw new ArgumentException($"Key: {defaultKey} does not exist");
            }

            if (dictionary.TryGetValue(key, out TVal response))
            {
                return response;
            }

            return dictionary[defaultKey];
        }
    }
}
