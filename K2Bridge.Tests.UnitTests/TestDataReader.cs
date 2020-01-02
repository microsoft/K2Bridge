// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace Tests
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Linq;

    public sealed class TestDataReader : IDataReader
    {
        private List<Dictionary<string, object>> items;
        private int index = -1;

        public TestDataReader(List<Dictionary<string, object>> items)
        {
            this.items = items;
        }

        public int Depth => 1;

        public bool IsClosed => false;

        public int RecordsAffected => 1;

        public int FieldCount => this.items[this.index].Count;

        public object this[int i] => this.items[this.index].ElementAt(i).Value;

        public object this[string name] => this.items[this.index][name];

        public void Close()
        {
        }

        public void Dispose()
        {
        }

        public bool GetBoolean(int i) => (bool)this[i];

        public byte GetByte(int i) => (byte)this[i];

        public long GetBytes(int i, long fieldOffset, byte[] buffer, int bufferoffset, int length)
        {
            throw new NotImplementedException();
        }

        public char GetChar(int i) => (char)this[i];

        public long GetChars(int i, long fieldoffset, char[] buffer, int bufferoffset, int length)
        {
            throw new NotImplementedException();
        }

        public IDataReader GetData(int i)
        {
            throw new NotImplementedException();
        }

        public string GetDataTypeName(int i) => this[i].GetType().Name;

        public DateTime GetDateTime(int i) => (DateTime)this[i];

        public decimal GetDecimal(int i) => (decimal)this[i];

        public double GetDouble(int i) => (double)this[i];

        public Type GetFieldType(int i) => this[i].GetType();

        public float GetFloat(int i) => (float)this[i];

        public Guid GetGuid(int i)
        {
            throw new NotImplementedException();
        }

        public short GetInt16(int i) => (short)this[i];

        public int GetInt32(int i) => (int)this[i];

        public long GetInt64(int i) => (long)this[i];

        public string GetName(int i) => this.items[this.index].Keys.ElementAt(i);

        public int GetOrdinal(string name)
        {
            throw new NotImplementedException();
        }

        public DataTable GetSchemaTable()
        {
            throw new NotImplementedException();
        }

        public string GetString(int i) => (string)this[i];

        public object GetValue(int i) => this[i];

        public int GetValues(object[] values)
        {
            throw new NotImplementedException();
        }

        public bool IsDBNull(int i)
        {
            if (this[i] == DBNull.Value)
            {
                return true;
            }

            return false;
        }

        public bool NextResult()
        {
            throw new NotImplementedException();
        }

        public bool Read()
        {
            this.index++;
            if (this.items.Count > this.index)
            {
                return true;
            }

            return false;
        }
    }
}