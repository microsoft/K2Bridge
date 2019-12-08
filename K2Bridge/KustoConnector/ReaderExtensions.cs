// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace K2Bridge.KustoConnector
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using K2Bridge.Models.Response;

    internal static class ReaderExtensions
    {
        private static Dictionary<Type, Func<IDataRecord, int, object>> readerSwitch = new Dictionary<Type, Func<IDataRecord, int, object>>
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

        private static Random random = new Random();

        internal static IEnumerable<Hit> ReadHits(this IDataReader reader, QueryData query)
        {
            while (reader.Read())
            {
                var hit = Hit.Create((IDataRecord)reader, query);
                hit.Id = random.Next().ToString();
                yield return hit;
            }
        }

        internal static IEnumerable<IBucket> ReadAggs(this IDataReader reader)
        {
            while (reader.Read())
            {
                var bucket = BucketFactory.MakeBucket((IDataRecord)reader);
                yield return bucket;
            }
        }

        internal static object ReadValue(this IDataRecord record, int index) =>
            readerSwitch.GetDictionaryValueOrDefault(
                                record.GetFieldType(index),
                                typeof(object))(record, index);

        internal static object ReadValueOrDbNull(this IDataRecord record, int index, Func<object> readFunc) =>
            record.IsDBNull(index) ? (object)null : readFunc();

        private static TVal GetDictionaryValueOrDefault<TKey, TVal>(this Dictionary<TKey, TVal> dictionary, TKey key, TKey defaultKey)
        {
            TVal response;
            if (!dictionary.ContainsKey(defaultKey))
            {
                throw new ArgumentException("default key does not exist");
            }

            if (dictionary.TryGetValue(key, out response))
            {
                return response;
            }

            return dictionary[defaultKey];
        }
    }
}
