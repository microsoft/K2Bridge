// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2Bridge.Tests.UnitTests.KustoDAL;

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

public sealed class DataReaderMock : IDataReader
{
    private readonly List<Dictionary<string, object>> items;
    private int index = -1;

    public DataReaderMock(List<Dictionary<string, object>> items)
    {
        this.items = items;
    }

    public int Depth => 1;

    public bool IsClosed => false;

    public int RecordsAffected => 1;

    public int FieldCount => items[index].Count;

    public object this[int i] => items[index].ElementAt(i).Value;

    public object this[string name] => items[index][name];

    public void Close()
    {
    }

    public void Dispose()
    {
    }

    public bool GetBoolean(int i)
    {
        return (bool)this[i];
    }

    public byte GetByte(int i)
    {
        return (byte)this[i];
    }

    public long GetBytes(int i, long fieldOffset, byte[] buffer, int bufferoffset, int length)
    {
        throw new NotImplementedException();
    }

    public char GetChar(int i)
    {
        return (char)this[i];
    }

    public long GetChars(int i, long fieldoffset, char[] buffer, int bufferoffset, int length)
    {
        throw new NotImplementedException();
    }

    public IDataReader GetData(int i)
    {
        throw new NotImplementedException();
    }

    public string GetDataTypeName(int i)
    {
        return this[i].GetType().Name;
    }

    public DateTime GetDateTime(int i)
    {
        return (DateTime)this[i];
    }

    public decimal GetDecimal(int i)
    {
        return (decimal)this[i];
    }

    public double GetDouble(int i)
    {
        return (double)this[i];
    }

    public Type GetFieldType(int i)
    {
        return this[i].GetType();
    }

    public float GetFloat(int i)
    {
        return (float)this[i];
    }

    public Guid GetGuid(int i)
    {
        throw new NotImplementedException();
    }

    public short GetInt16(int i)
    {
        return (short)this[i];
    }

    public int GetInt32(int i)
    {
        return (int)this[i];
    }

    public long GetInt64(int i)
    {
        return (long)this[i];
    }

    public string GetName(int i)
    {
        return items[index].Keys.ElementAt(i);
    }

    public int GetOrdinal(string name)
    {
        throw new NotImplementedException();
    }

    public DataTable GetSchemaTable()
    {
        throw new NotImplementedException();
    }

    public string GetString(int i)
    {
        return (string)this[i];
    }

    public object GetValue(int i)
    {
        return this[i];
    }

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
        index++;
        if (items.Count > index)
        {
            return true;
        }

        return false;
    }
}
