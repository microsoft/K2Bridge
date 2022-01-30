// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2Bridge.Tests.UnitTests.Models.Response;

using System;
using K2Bridge.Utils;
using NUnit.Framework;

[TestFixture]
public class TimeUtilsTests
{
    [Test]
    public void ToEpochMilliseconds_WithValidInput_ReturnsEpochTime()
    {
        var millis = TimeUtils.ToEpochMilliseconds(new DateTime(2017, 02, 01, 03, 04, 05, DateTimeKind.Utc));
        Assert.AreEqual(1485918245000, millis);
        millis = TimeUtils.ToEpochMilliseconds(new DateTime(2018, 12, 11, 23, 34, 45, 632, DateTimeKind.Utc));
        Assert.AreEqual(1544571285632, millis);
    }
}
