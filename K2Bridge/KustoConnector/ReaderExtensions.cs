namespace K2Bridge.KustoConnector
{
    using System;
    using System.Collections.Generic;
    using System.Data;

    internal static class ReaderExtensions
    {
        private static Dictionary<Type, Func<IDataRecord, int, object>> readerSwitch = new Dictionary<Type, Func<IDataRecord, int, object>>
        {
            { typeof(bool), (reader, index) => reader.GetBoolean(index) },
            { typeof(sbyte), (reader, index) => (sbyte)reader.GetValue(index) == 0 ? false : true },
            { typeof(short), (reader, index) => reader.GetInt16(index) },
            { typeof(int), (reader, index) => reader.GetInt32(index) },
            { typeof(long), (reader, index) => reader.GetInt64(index) },
            { typeof(double), (reader, index) => reader.GetDouble(index) },
            { typeof(string), (reader, index) => reader.GetString(index) },
            { typeof(DateTime), (reader, index) => reader.GetDateTime(index) },
            { typeof(decimal), (reader, index) => reader.GetDecimal(index) },
            { typeof(object), (reader, index) => reader.GetValue(index) },
        };

        internal static IEnumerable<Hit> ReadHits(this IDataReader reader, string indexName)
        {
            while (reader.Read())
            {
                yield return Hit.Create((IDataRecord)reader, indexName);
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
