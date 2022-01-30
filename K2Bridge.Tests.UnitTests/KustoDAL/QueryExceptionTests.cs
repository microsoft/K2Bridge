// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

using System;
using K2Bridge.KustoDAL;
using NUnit.Framework;

namespace K2Bridge.Tests.UnitTests.KustoDAL;

[TestFixture]
public class QueryExceptionTests
{
    [Test]
    public void Constructor_WithNoArgument_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => new QueryException());
    }

    [Test]
    public void Constructor_WithMessage_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => new QueryException("test"));
    }

    [Test]
    public void Constructor_WithInnerExceptionAndMessage_ConstructsQueryException()
    {
        var exc = new QueryException("test", new ArgumentException("test"));
        Assert.AreEqual(QueryException.QueryPhaseName, exc.PhaseName);
    }
}
