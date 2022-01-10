// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace UnitTests.K2Bridge.Visitors
{
    using System.Collections.Generic;
    using global::K2Bridge.Models.Request.Aggregations;
    using global::K2Bridge.Tests.UnitTests.Visitors;
    using global::K2Bridge.Visitors;
    using NUnit.Framework;

    [TestFixture]
    public class DateMathParserTests
    {
        [TestCase("2018", ExpectedResult = "make_datetime('2018')")]
        [TestCase("2018-01", ExpectedResult = "make_datetime('2018-01')")]
        [TestCase("2018-01-01", ExpectedResult = "make_datetime('2018-01-01')")]
        [TestCase("2018-01-01T14:42:30", ExpectedResult = "make_datetime('2018-01-01T14:42:30')")]
        [TestCase("2018-01-01||+1y", ExpectedResult = "datetime_add('year', 1, make_datetime('2018-01-01'))")]
        [TestCase("2018-01-01||+1y+1d", ExpectedResult = "datetime_add('day', 1, datetime_add('year', 1, make_datetime('2018-01-01')))")]
        [TestCase("2018-01-01||+1M-1d", ExpectedResult = "datetime_add('day', -1, datetime_add('month', 1, make_datetime('2018-01-01')))")]
        [TestCase("2018-01-01||+1y-1M+1d", ExpectedResult = "datetime_add('day', 1, datetime_add('month', -1, datetime_add('year', 1, make_datetime('2018-01-01'))))")]
        [TestCase("2018-01-01||+12M", ExpectedResult = "datetime_add('month', 12, make_datetime('2018-01-01'))")]
        [TestCase("2018-01-01||+8w", ExpectedResult = "datetime_add('week', 8, make_datetime('2018-01-01'))")]
        [TestCase("2018-01-01||+1d", ExpectedResult = "datetime_add('day', 1, make_datetime('2018-01-01'))")]
        [TestCase("2018-01-01||-2y", ExpectedResult = "datetime_add('year', -2, make_datetime('2018-01-01'))")]
        [TestCase("2018-01-01||-20M", ExpectedResult = "datetime_add('month', -20, make_datetime('2018-01-01'))")]
        [TestCase("2018-01-01||-2w", ExpectedResult = "datetime_add('week', -2, make_datetime('2018-01-01'))")]
        [TestCase("2018-01-01||-200d", ExpectedResult = "datetime_add('day', -200, make_datetime('2018-01-01'))")]
        [TestCase("2018-01-01T14:42:30||+10h", ExpectedResult = "datetime_add('hour', 10, make_datetime('2018-01-01T14:42:30'))")]
        [TestCase("2018-01-01T14:42:30||-1H", ExpectedResult = "datetime_add('hour', -1, make_datetime('2018-01-01T14:42:30'))")]
        [TestCase("2018-01-01T14:42:30||+012m", ExpectedResult = "datetime_add('minute', 12, make_datetime('2018-01-01T14:42:30'))")]
        [TestCase("2018-01-01T14:42:30||-015s", ExpectedResult = "datetime_add('second', -15, make_datetime('2018-01-01T14:42:30'))")]
        [TestCase("now", ExpectedResult = "now()")]
        [TestCase("now+1y", ExpectedResult = "datetime_add('year', 1, now())")]
        [TestCase("now+12M", ExpectedResult = "datetime_add('month', 12, now())")]
        [TestCase("now+8w", ExpectedResult = "datetime_add('week', 8, now())")]
        [TestCase("now+0d", ExpectedResult = "datetime_add('day', 0, now())")]
        [TestCase("now-1y", ExpectedResult = "datetime_add('year', -1, now())")]
        [TestCase("now-12M", ExpectedResult = "datetime_add('month', -12, now())")]
        [TestCase("now-08w", ExpectedResult = "datetime_add('week', -8, now())")]
        [TestCase("now-10d", ExpectedResult = "datetime_add('day', -10, now())")]
        [TestCase("now-10d+3h", ExpectedResult = "datetime_add('hour', 3, datetime_add('day', -10, now()))")]
        [TestCase("2018-01-01||+3y/y", ExpectedResult = "startofyear(datetime_add('year', 3, make_datetime('2018-01-01')))")]
        [TestCase("2018-01-01||+1M/y", ExpectedResult = "startofyear(datetime_add('month', 1, make_datetime('2018-01-01')))")]
        [TestCase("2018-01-01||-30w/y", ExpectedResult = "startofyear(datetime_add('week', -30, make_datetime('2018-01-01')))")]
        [TestCase("2018-01-01||+3d/y", ExpectedResult = "startofyear(datetime_add('day', 3, make_datetime('2018-01-01')))")]
        [TestCase("2018-01-01||-2y/M", ExpectedResult = "startofmonth(datetime_add('year', -2, make_datetime('2018-01-01')))")]
        [TestCase("2018-01-01||-12M/w", ExpectedResult = "startofweek(datetime_add('month', -12, make_datetime('2018-01-01')))")]
        [TestCase("2018-01-01||+02m/d", ExpectedResult = "startofday(datetime_add('minute', 2, make_datetime('2018-01-01')))")]
        [TestCase("2018-01-01||+1y-1M/d", ExpectedResult = "startofday(datetime_add('month', -1, datetime_add('year', 1, make_datetime('2018-01-01'))))")]
        [TestCase("now+1y/M", ExpectedResult = "startofmonth(datetime_add('year', 1, now()))")]
        [TestCase("now+12M/w", ExpectedResult = "startofweek(datetime_add('month', 12, now()))")]
        [TestCase("now+10M/d", ExpectedResult = "startofday(datetime_add('month', 10, now()))")]
        [TestCase("now+8w/d", ExpectedResult = "startofday(datetime_add('week', 8, now()))")]
        [TestCase("now-1y/M", ExpectedResult = "startofmonth(datetime_add('year', -1, now()))")]
        [TestCase("now-12M/w", ExpectedResult = "startofweek(datetime_add('month', -12, now()))")]
        [TestCase("now-08w/d", ExpectedResult = "startofday(datetime_add('week', -8, now()))")]
        [TestCase("2018-01-01T14:42:30||+10h/m", ExpectedResult = "bin(datetime_add('hour', 10, make_datetime('2018-01-01T14:42:30')), 1m)")]
        [TestCase("2018-01-01T14:42:30||-12h/m", ExpectedResult = "bin(datetime_add('hour', -12, make_datetime('2018-01-01T14:42:30')), 1m)")]
        [TestCase("2018-01-01T14:42:30||+10h/s", ExpectedResult = "bin(datetime_add('hour', 10, make_datetime('2018-01-01T14:42:30')), 1s)")]
        [TestCase("2018-01-01T14:42:30||-12h/s", ExpectedResult = "bin(datetime_add('hour', -12, make_datetime('2018-01-01T14:42:30')), 1s)")]
        [TestCase("2018-01-01T14:42:30||-12h+3m/s", ExpectedResult = "bin(datetime_add('minute', 3, datetime_add('hour', -12, make_datetime('2018-01-01T14:42:30'))), 1s)")]
        [TestCase("2018-01-01||+1d/h", ExpectedResult = "bin(datetime_add('day', 1, make_datetime('2018-01-01')), 1h)")]
        [TestCase("2018-01-01||-2d/H", ExpectedResult = "bin(datetime_add('day', -2, make_datetime('2018-01-01')), 1h)")]
        [TestCase("2018-01-01||+01d/m", ExpectedResult = "bin(datetime_add('day', 1, make_datetime('2018-01-01')), 1m)")]
        [TestCase("2018-01-01||-012d/m", ExpectedResult = "bin(datetime_add('day', -12, make_datetime('2018-01-01')), 1m)")]
        [TestCase("2018-01-01||+10d/s", ExpectedResult = "bin(datetime_add('day', 10, make_datetime('2018-01-01')), 1s)")]
        [TestCase("2018-01-01||-1d/s", ExpectedResult = "bin(datetime_add('day', -1, make_datetime('2018-01-01')), 1s)")]
        public string ParseDateMath_ReturnsValidResponse(string expr)
        {
            return DateMathParser.ParseDateMath(expr);
        }
    }
}
