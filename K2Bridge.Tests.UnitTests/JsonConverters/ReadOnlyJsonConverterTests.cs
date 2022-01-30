// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

using System;
using K2Bridge.JsonConverters;
using NUnit.Framework;

namespace K2Bridge.Tests.UnitTests.JsonConverters;

[TestFixture]
public class ReadOnlyJsonConverterTests
{
    [Test]
    public void ReadOnlyJsonConverter_CanWrite_ReturnsFalse()
    {
        var readOnlyConverter = new QueryStringClauseConverter();
        Assert.IsFalse(readOnlyConverter.CanWrite);
    }

    [Test]
    public void ReadOnlyJsonConverter_NotImplementedMethods_ThrowsAsExpected()
    {
        var readOnlyConverter = new QueryStringClauseConverter();
        Assert.Throws<NotImplementedException>(() => readOnlyConverter.CanConvert(typeof(string)));
        Assert.Throws<NotImplementedException>(() => readOnlyConverter.WriteJson(null, null, null));
    }
}
