// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2Bridge.Tests.UnitTests.Visitors;

using K2Bridge.Models.Request.Queries;
using NUnit.Framework;

public enum RangeType
{
    Wildcard,
    Exclusive,
    Inclusive,
}

[TestFixture]
public class RangeClauseVisitorTests
{
    // Numeric RangeClause Query Tests
    [TestCase(RangeType.Wildcard, RangeType.Wildcard, ExpectedResult = "isnotnull(['MyField'])", TestName = "Visit_WithValidRangeBetweenNumbers_ReturnsValidResponse_WhenMinWildcardAndMaxWildcard")]
    [TestCase(RangeType.Wildcard, RangeType.Exclusive, ExpectedResult = "['MyField'] < 10", TestName = "Visit_WithValidRangeBetweenNumbers_ReturnsValidResponse_WhenMinWildcardAndMaxOpen")]
    [TestCase(RangeType.Wildcard, RangeType.Inclusive, ExpectedResult = "['MyField'] <= 10", TestName = "Visit_WithValidRangeBetweenNumbers_ReturnsValidResponse_WhenMinWildcardAndMaxClosed")]
    [TestCase(RangeType.Exclusive, RangeType.Wildcard, ExpectedResult = "['MyField'] > 0", TestName = "Visit_WithValidRangeBetweenNumbers_ReturnsValidResponse_WhenMinOpenAndMaxWildcard")]
    [TestCase(RangeType.Exclusive, RangeType.Exclusive, ExpectedResult = "['MyField'] > 0 and ['MyField'] < 10", TestName = "Visit_WithValidRangeBetweenNumbers_ReturnsValidResponse_WhenMinOpenAndMaxOpen")]
    [TestCase(RangeType.Exclusive, RangeType.Inclusive, ExpectedResult = "['MyField'] > 0 and ['MyField'] <= 10", TestName = "Visit_WithValidRangeBetweenNumbers_ReturnsValidResponse_WhenMinOpenAndMaxClosed")]
    [TestCase(RangeType.Inclusive, RangeType.Wildcard, ExpectedResult = "['MyField'] >= 0", TestName = "Visit_WithValidRangeBetweenNumbers_ReturnsValidResponse_WhenMinClosedAndMaxWildcard")]
    [TestCase(RangeType.Inclusive, RangeType.Exclusive, ExpectedResult = "['MyField'] >= 0 and ['MyField'] < 10", TestName = "Visit_WithValidRangeBetweenNumbers_ReturnsValidResponse_WhenMinClosedAndMaxOpen")]
    [TestCase(RangeType.Inclusive, RangeType.Inclusive, ExpectedResult = "['MyField'] >= 0 and ['MyField'] <= 10", TestName = "Visit_WithValidRangeBetweenNumbers_ReturnsValidResponse_WhenMinClosedAndMaxClosed")]
    public string TestValidRangeClauseVisitNumberBetweenTwoInts(RangeType minRange, RangeType maxRange)
    {
        return RangeClauseToKQL(CreateRangeClause("0", "10", minRange, maxRange));
    }

    [TestCase(RangeType.Wildcard, RangeType.Wildcard, ExpectedResult = "isnotnull(['MyField'])", TestName = "Visit_WithValidRangeBetweenIntAndDecimal_ReturnsValidResponse_WhenMinWildcardAndMaxWildcard")]
    [TestCase(RangeType.Wildcard, RangeType.Exclusive, ExpectedResult = "['MyField'] < 10.10", TestName = "Visit_WithValidRangeBetweenIntAndDecimal_ReturnsValidResponse_WhenMinWildcardAndMaxOpen")]
    [TestCase(RangeType.Wildcard, RangeType.Inclusive, ExpectedResult = "['MyField'] <= 10.10", TestName = "Visit_WithValidRangeBetweenIntAndDecimal_ReturnsValidResponse_WhenMinWildcardAndMaxClosed")]
    [TestCase(RangeType.Exclusive, RangeType.Wildcard, ExpectedResult = "['MyField'] > 0", TestName = "Visit_WithValidRangeBetweenIntAndDecimal_ReturnsValidResponse_WhenMinOpenAndMaxWildcard")]
    [TestCase(RangeType.Exclusive, RangeType.Exclusive, ExpectedResult = "['MyField'] > 0 and ['MyField'] < 10.10", TestName = "Visit_WithValidRangeBetweenIntAndDecimal_ReturnsValidResponse_WhenMinOpenAndMaxOpen")]
    [TestCase(RangeType.Exclusive, RangeType.Inclusive, ExpectedResult = "['MyField'] > 0 and ['MyField'] <= 10.10", TestName = "Visit_WithValidRangeBetweenIntAndDecimal_ReturnsValidResponse_WhenMinOpenAndMaxClosed")]
    [TestCase(RangeType.Inclusive, RangeType.Wildcard, ExpectedResult = "['MyField'] >= 0", TestName = "Visit_WithValidRangeBetweenIntAndDecimal_ReturnsValidResponse_WhenMinClosedAndMaxWildcard")]
    [TestCase(RangeType.Inclusive, RangeType.Exclusive, ExpectedResult = "['MyField'] >= 0 and ['MyField'] < 10.10", TestName = "Visit_WithValidRangeBetweenIntAndDecimal_ReturnsValidResponse_WhenMinClosedAndMaxOpen")]
    [TestCase(RangeType.Inclusive, RangeType.Inclusive, ExpectedResult = "['MyField'] >= 0 and ['MyField'] <= 10.10", TestName = "Visit_WithValidRangeBetweenIntAndDecimal_ReturnsValidResponse_WhenMinClosedAndMaxClosed")]
    public string TestValidRangeClauseVisitNumberBetweenIntAndDecimal(RangeType minRange, RangeType maxRange)
    {
        return RangeClauseToKQL(CreateRangeClause("0", "10.10", minRange, maxRange));
    }

    [TestCase(RangeType.Wildcard, RangeType.Wildcard, ExpectedResult = "isnotnull(['MyField'])", TestName = "Visit_WithValidRangeBetweenDecimals_ReturnsValidResponse_WhenMinWildcardAndMaxWildcard")]
    [TestCase(RangeType.Wildcard, RangeType.Exclusive, ExpectedResult = "['MyField'] < 20.20", TestName = "Visit_WithValidRangeBetweenDecimals_ReturnsValidResponse_WhenMinWildcardAndMaxOpen")]
    [TestCase(RangeType.Wildcard, RangeType.Inclusive, ExpectedResult = "['MyField'] <= 20.20", TestName = "Visit_WithValidRangeBetweenDecimals_ReturnsValidResponse_WhenMinWildcardAndMaxClosed")]
    [TestCase(RangeType.Exclusive, RangeType.Wildcard, ExpectedResult = "['MyField'] > 10.10", TestName = "Visit_WithValidRangeBetweenDecimals_ReturnsValidResponse_WhenMinOpenAndMaxWildcard")]
    [TestCase(RangeType.Exclusive, RangeType.Exclusive, ExpectedResult = "['MyField'] > 10.10 and ['MyField'] < 20.20", TestName = "Visit_WithValidRangeBetweenDecimals_ReturnsValidResponse_WhenMinOpenAndMaxOpen")]
    [TestCase(RangeType.Exclusive, RangeType.Inclusive, ExpectedResult = "['MyField'] > 10.10 and ['MyField'] <= 20.20", TestName = "Visit_WithValidRangeBetweenDecimals_ReturnsValidResponse_WhenMinOpenAndMaxClosed")]
    [TestCase(RangeType.Inclusive, RangeType.Wildcard, ExpectedResult = "['MyField'] >= 10.10", TestName = "Visit_WithValidRangeBetweenDecimals_ReturnsValidResponse_WhenMinClosedAndMaxWildcard")]
    [TestCase(RangeType.Inclusive, RangeType.Exclusive, ExpectedResult = "['MyField'] >= 10.10 and ['MyField'] < 20.20", TestName = "Visit_WithValidRangeBetweenDecimals_ReturnsValidResponse_WhenMinClosedAndMaxOpen")]
    [TestCase(RangeType.Inclusive, RangeType.Inclusive, ExpectedResult = "['MyField'] >= 10.10 and ['MyField'] <= 20.20", TestName = "Visit_WithValidRangeBetweenDecimals_ReturnsValidResponse_WhenMinClosedAndMaxClosed")]
    public string TestValidRangeClauseVisitNumberBetweenTwoDecimals(RangeType minRange, RangeType maxRange)
    {
        return RangeClauseToKQL(CreateRangeClause("10.10", "20.20", minRange, maxRange));
    }

    // Time RangeClause Query Tests
    [TestCase(RangeType.Wildcard, RangeType.Wildcard, ExpectedResult = "isnotnull(['MyField'])", TestName = "Visit_WithValidRangeBetweenInts_ReturnsValidResponse_WhenMinWildcardAndMaxWildcard")]
    [TestCase(RangeType.Wildcard, RangeType.Exclusive, ExpectedResult = "['MyField'] < unixtime_milliseconds_todatetime(10)", TestName = "Visit_WithValidRangeBetweenInts_ReturnsValidResponse_WhenMinWildcardAndMaxOpen")]
    [TestCase(RangeType.Wildcard, RangeType.Inclusive, ExpectedResult = "['MyField'] <= unixtime_milliseconds_todatetime(10)", TestName = "Visit_WithValidRangeBetweenInts_ReturnsValidResponse_WhenMinWildcardAndMaxClosed")]
    [TestCase(RangeType.Exclusive, RangeType.Wildcard, ExpectedResult = "['MyField'] > unixtime_milliseconds_todatetime(0)", TestName = "Visit_WithValidRangeBetweenInts_ReturnsValidResponse_WhenMinOpenAndMaxWildcard")]
    [TestCase(RangeType.Exclusive, RangeType.Exclusive, ExpectedResult = "['MyField'] > unixtime_milliseconds_todatetime(0) and ['MyField'] < unixtime_milliseconds_todatetime(10)", TestName = "Visit_WithValidRangeBetweenInts_ReturnsValidResponse_WhenMinOpenAndMaxOpen")]
    [TestCase(RangeType.Exclusive, RangeType.Inclusive, ExpectedResult = "['MyField'] > unixtime_milliseconds_todatetime(0) and ['MyField'] <= unixtime_milliseconds_todatetime(10)", TestName = "Visit_WithValidRangeBetweenInts_ReturnsValidResponse_WhenMinOpenAndMaxClosed")]
    [TestCase(RangeType.Inclusive, RangeType.Wildcard, ExpectedResult = "['MyField'] >= unixtime_milliseconds_todatetime(0)", TestName = "Visit_WithValidRangeBetweenInts_ReturnsValidResponse_WhenMinClosedAndMaxWildcard")]
    [TestCase(RangeType.Inclusive, RangeType.Exclusive, ExpectedResult = "['MyField'] >= unixtime_milliseconds_todatetime(0) and ['MyField'] < unixtime_milliseconds_todatetime(10)", TestName = "Visit_WithValidRangeBetweenInts_ReturnsValidResponse_WhenMinClosedAndMaxOpen")]
    [TestCase(RangeType.Inclusive, RangeType.Inclusive, ExpectedResult = "['MyField'] >= unixtime_milliseconds_todatetime(0) and ['MyField'] <= unixtime_milliseconds_todatetime(10)", TestName = "Visit_WithValidRangeBetweenInts_ReturnsValidResponse_WhenMinClosedAndMaxClosed")]
    public string TestValidTimeRangeClauseVisitNumberBetweenTwoInts(RangeType minRange, RangeType maxRange)
    {
        return RangeClauseToKQL(CreateTimeRangeClause("0", "10", minRange, maxRange));
    }

    [TestCase(RangeType.Wildcard, RangeType.Wildcard, ExpectedResult = "isnotnull(['MyField'])", TestName = "Visit_WithValidTimeRangeBetweenNumbers_ReturnsValidResponse_WhenMinWildcardAndMaxWildcard")]
    [TestCase(RangeType.Wildcard, RangeType.Exclusive, ExpectedResult = "['MyField'] < unixtime_milliseconds_todatetime(10.10)", TestName = "Visit_WithValidTimeRangeBetweenNumbers_ReturnsValidResponse_WhenMinWildcardAndMaxOpen")]
    [TestCase(RangeType.Wildcard, RangeType.Inclusive, ExpectedResult = "['MyField'] <= unixtime_milliseconds_todatetime(10.10)", TestName = "Visit_WithValidTimeRangeBetweenNumbers_ReturnsValidResponse_WhenMinWildcardAndMaxClosed")]
    [TestCase(RangeType.Exclusive, RangeType.Wildcard, ExpectedResult = "['MyField'] > unixtime_milliseconds_todatetime(0)", TestName = "Visit_WithValidTimeRangeBetweenNumbers_ReturnsValidResponse_WhenMinOpenAndMaxWildcard")]
    [TestCase(RangeType.Exclusive, RangeType.Exclusive, ExpectedResult = "['MyField'] > unixtime_milliseconds_todatetime(0) and ['MyField'] < unixtime_milliseconds_todatetime(10.10)", TestName = "Visit_WithValidTimeRangeBetweenNumbers_ReturnsValidResponse_WhenMinOpenAndMaxOpen")]
    [TestCase(RangeType.Exclusive, RangeType.Inclusive, ExpectedResult = "['MyField'] > unixtime_milliseconds_todatetime(0) and ['MyField'] <= unixtime_milliseconds_todatetime(10.10)", TestName = "Visit_WithValidTimeRangeBetweenNumbers_ReturnsValidResponse_WhenMinOpenAndMaxClosed")]
    [TestCase(RangeType.Inclusive, RangeType.Wildcard, ExpectedResult = "['MyField'] >= unixtime_milliseconds_todatetime(0)", TestName = "Visit_WithValidTimeRangeBetweenNumbers_ReturnsValidResponse_WhenMinClosedAndMaxWildcard")]
    [TestCase(RangeType.Inclusive, RangeType.Exclusive, ExpectedResult = "['MyField'] >= unixtime_milliseconds_todatetime(0) and ['MyField'] < unixtime_milliseconds_todatetime(10.10)", TestName = "Visit_WithValidTimeRangeBetweenNumbers_ReturnsValidResponse_WhenMinClosedAndMaxOpen")]
    [TestCase(RangeType.Inclusive, RangeType.Inclusive, ExpectedResult = "['MyField'] >= unixtime_milliseconds_todatetime(0) and ['MyField'] <= unixtime_milliseconds_todatetime(10.10)", TestName = "Visit_WithValidTimeRangeBetweenNumbers_ReturnsValidResponse_WhenMinClosedAndMaxClosed")]
    public string TestValidTimeRangeClauseVisitNumberBetweenIntAndDecimal(RangeType minRange, RangeType maxRange)
    {
        return RangeClauseToKQL(CreateTimeRangeClause("0", "10.10", minRange, maxRange));
    }

    [TestCase(RangeType.Wildcard, RangeType.Wildcard, ExpectedResult = "isnotnull(['MyField'])", TestName = "Visit_WithValidTimeRangeBetweenDecimals_ReturnsValidResponse_WhenMinWildcardAndMaxWildcard")]
    [TestCase(RangeType.Wildcard, RangeType.Exclusive, ExpectedResult = "['MyField'] < unixtime_milliseconds_todatetime(20.20)", TestName = "Visit_WithValidTimeRangeBetweenDecimals_ReturnsValidResponse_WhenMinWildcardAndMaxOpen")]
    [TestCase(RangeType.Wildcard, RangeType.Inclusive, ExpectedResult = "['MyField'] <= unixtime_milliseconds_todatetime(20.20)", TestName = "Visit_WithValidTimeRangeBetweenDecimals_ReturnsValidResponse_WhenMinWildcardAndMaxClosed")]
    [TestCase(RangeType.Exclusive, RangeType.Wildcard, ExpectedResult = "['MyField'] > unixtime_milliseconds_todatetime(10.10)", TestName = "Visit_WithValidTimeRangeBetweenDecimals_ReturnsValidResponse_WhenMinOpenAndMaxWildcard")]
    [TestCase(RangeType.Exclusive, RangeType.Exclusive, ExpectedResult = "['MyField'] > unixtime_milliseconds_todatetime(10.10) and ['MyField'] < unixtime_milliseconds_todatetime(20.20)", TestName = "Visit_WithValidTimeRangeBetweenDecimals_ReturnsValidResponse_WhenMinOpenAndMaxOpen")]
    [TestCase(RangeType.Exclusive, RangeType.Inclusive, ExpectedResult = "['MyField'] > unixtime_milliseconds_todatetime(10.10) and ['MyField'] <= unixtime_milliseconds_todatetime(20.20)", TestName = "Visit_WithValidTimeRangeBetweenDecimals_ReturnsValidResponse_WhenMinOpenAndMaxClosed")]
    [TestCase(RangeType.Inclusive, RangeType.Wildcard, ExpectedResult = "['MyField'] >= unixtime_milliseconds_todatetime(10.10)", TestName = "Visit_WithValidTimeRangeBetweenDecimals_ReturnsValidResponse_WhenMinClosedAndMaxWildcard")]
    [TestCase(RangeType.Inclusive, RangeType.Exclusive, ExpectedResult = "['MyField'] >= unixtime_milliseconds_todatetime(10.10) and ['MyField'] < unixtime_milliseconds_todatetime(20.20)", TestName = "Visit_WithValidTimeRangeBetweenDecimals_ReturnsValidResponse_WhenMinClosedAndMaxOpen")]
    [TestCase(RangeType.Inclusive, RangeType.Inclusive, ExpectedResult = "['MyField'] >= unixtime_milliseconds_todatetime(10.10) and ['MyField'] <= unixtime_milliseconds_todatetime(20.20)", TestName = "Visit_WithValidTimeRangeBetweenDecimals_ReturnsValidResponse_WhenMinClosedAndMaxClosed")]
    public string TestValidTimeRangeClauseVisitNumberBetweenTwoDecimals(RangeType minRange, RangeType maxRange)
    {
        return RangeClauseToKQL(CreateTimeRangeClause("10.10", "20.20", minRange, maxRange));
    }

    [TestCase(RangeType.Wildcard, RangeType.Wildcard, ExpectedResult = "isnotnull(['MyField'])", TestName = "Visit_ValidDateRange_ReturnsValidResponse_WhenMinWildcardAndMaxWildcard")]
    [TestCase(RangeType.Wildcard, RangeType.Exclusive, ExpectedResult = "['MyField'] < todatetime(\"2020-02-22T10:00:00.0000000\")", TestName = "Visit_ValidDateRange_ReturnsValidResponse_WhenMinWildcardAndMaxOpen")]
    [TestCase(RangeType.Wildcard, RangeType.Inclusive, ExpectedResult = "['MyField'] <= todatetime(\"2020-02-22T10:00:00.0000000\")", TestName = "Visit_ValidDateRange_ReturnsValidResponse_WhenMinWildcardAndMaxClosed")]
    [TestCase(RangeType.Exclusive, RangeType.Wildcard, ExpectedResult = "['MyField'] > todatetime(\"2020-01-01T00:00:00.0000000\")", TestName = "Visit_ValidDateRange_ReturnsValidResponse_WhenMinOpenAndMaxWildcard")]
    [TestCase(RangeType.Exclusive, RangeType.Exclusive, ExpectedResult = "['MyField'] > todatetime(\"2020-01-01T00:00:00.0000000\") and ['MyField'] < todatetime(\"2020-02-22T10:00:00.0000000\")", TestName = "Visit_ValidDateRange_ReturnsValidResponse_WhenMinOpenAndMaxOpen")]
    [TestCase(RangeType.Exclusive, RangeType.Inclusive, ExpectedResult = "['MyField'] > todatetime(\"2020-01-01T00:00:00.0000000\") and ['MyField'] <= todatetime(\"2020-02-22T10:00:00.0000000\")", TestName = "Visit_ValidDateRange_ReturnsValidResponse_WhenMinOpenAndMaxClosed")]
    [TestCase(RangeType.Inclusive, RangeType.Wildcard, ExpectedResult = "['MyField'] >= todatetime(\"2020-01-01T00:00:00.0000000\")", TestName = "Visit_ValidDateRange_ReturnsValidResponse_WhenMinClosedAndMaxWildcard")]
    [TestCase(RangeType.Inclusive, RangeType.Exclusive, ExpectedResult = "['MyField'] >= todatetime(\"2020-01-01T00:00:00.0000000\") and ['MyField'] < todatetime(\"2020-02-22T10:00:00.0000000\")", TestName = "Visit_ValidDateRange_ReturnsValidResponse_WhenMinClosedAndMaxOpen")]
    [TestCase(RangeType.Inclusive, RangeType.Inclusive, ExpectedResult = "['MyField'] >= todatetime(\"2020-01-01T00:00:00.0000000\") and ['MyField'] <= todatetime(\"2020-02-22T10:00:00.0000000\")", TestName = "Visit_ValidDateRange_ReturnsValidResponse_WhenMinClosedAndMaxClosed")]
    public string TestValidDateRange_ReturnsValidResponse(RangeType minRange, RangeType maxRange)
    {
        return DateRangeClauseToKQL(CreateDateRangeClause("2020-01-01 00:00", "2020-02-22 10:00", minRange, maxRange));
    }

    // Numeric RangeClause Query Tests
    [TestCase(RangeType.Wildcard, RangeType.Wildcard, ExpectedResult = "isnotnull(['MyField'])", TestName = "Visit_ValidTextRange_ReturnsValidResponse_WhenMinWildcardAndMaxWildcard")]
    [TestCase(RangeType.Wildcard, RangeType.Exclusive, ExpectedResult = "0 < strcmp('C', tostring(['MyField']))", TestName = "Visit_ValidTextRange_ReturnsValidResponse_WhenMinWildcardAndMaxOpen")]
    [TestCase(RangeType.Wildcard, RangeType.Inclusive, ExpectedResult = "0 <= strcmp('C', tostring(['MyField']))", TestName = "Visit_ValidTextRange_ReturnsValidResponse_WhenMinWildcardAndMaxClosed")]
    [TestCase(RangeType.Exclusive, RangeType.Wildcard, ExpectedResult = "0 > strcmp('A', tostring(['MyField']))", TestName = "Visit_ValidTextRange_ReturnsValidResponse_WhenMinOpenAndMaxWildcard")]
    [TestCase(RangeType.Exclusive, RangeType.Exclusive, ExpectedResult = "0 > strcmp('A', tostring(['MyField'])) and 0 < strcmp('C', tostring(['MyField']))", TestName = "Visit_ValidTextRange_ReturnsValidResponse_WhenMinOpenAndMaxOpen")]
    [TestCase(RangeType.Exclusive, RangeType.Inclusive, ExpectedResult = "0 > strcmp('A', tostring(['MyField'])) and 0 <= strcmp('C', tostring(['MyField']))", TestName = "Visit_ValidTextRange_ReturnsValidResponse_WhenMinOpenAndMaxClosed")]
    [TestCase(RangeType.Inclusive, RangeType.Wildcard, ExpectedResult = "0 >= strcmp('A', tostring(['MyField']))", TestName = "Visit_ValidTextRange_ReturnsValidResponse_WhenMinClosedAndMaxWildcard")]
    [TestCase(RangeType.Inclusive, RangeType.Exclusive, ExpectedResult = "0 >= strcmp('A', tostring(['MyField'])) and 0 < strcmp('C', tostring(['MyField']))", TestName = "Visit_ValidTextRange_ReturnsValidResponse_WhenMinClosedAndMaxOpen")]
    [TestCase(RangeType.Inclusive, RangeType.Inclusive, ExpectedResult = "0 >= strcmp('A', tostring(['MyField'])) and 0 <= strcmp('C', tostring(['MyField']))", TestName = "Visit_ValidTextRange_ReturnsValidResponse_WhenMinClosedAndMaxClosed")]
    public string TestValidTextRange_ReturnsValidResponse(RangeType minRange, RangeType maxRange)
    {
        return TextRangeClauseToKQL(CreateRangeClause("A", "C", minRange, maxRange));
    }

    private static RangeClause CreateRangeClause(string min, string max, RangeType minRange, RangeType maxRange)
    {
        return new RangeClause()
        {
            FieldName = "MyField",
            GTEValue = minRange == RangeType.Wildcard ? "*" : minRange == RangeType.Inclusive ? min : null,
            GTValue = minRange == RangeType.Exclusive ? min : null,
            LTEValue = maxRange == RangeType.Wildcard ? "*" : maxRange == RangeType.Inclusive ? max : null,
            LTValue = maxRange == RangeType.Exclusive ? max : null,
            Format = null,
        };
    }

    private static RangeClause CreateTimeRangeClause(string min, string max, RangeType minRange, RangeType maxRange)
    {
        return new RangeClause()
        {
            FieldName = "MyField",
            GTEValue = minRange == RangeType.Wildcard ? "*" : minRange == RangeType.Inclusive ? min : null,
            GTValue = minRange == RangeType.Exclusive ? min : null,
            LTEValue = maxRange == RangeType.Wildcard ? "*" : maxRange == RangeType.Inclusive ? max : null,
            LTValue = maxRange == RangeType.Exclusive ? max : null,
            Format = "epoch_millis",
        };
    }

    private static RangeClause CreateDateRangeClause(string min, string max, RangeType minRange, RangeType maxRange)
    {
        return new RangeClause()
        {
            FieldName = "MyField",
            GTEValue = minRange == RangeType.Wildcard ? "*" : minRange == RangeType.Inclusive ? min : null,
            GTValue = minRange == RangeType.Exclusive ? min : null,
            LTEValue = maxRange == RangeType.Wildcard ? "*" : maxRange == RangeType.Inclusive ? max : null,
            LTValue = maxRange == RangeType.Exclusive ? max : null,
            Format = null,
        };
    }

    private static string RangeClauseToKQL(RangeClause rangeClause)
    {
        var visitor = VisitorTestsUtils.CreateAndVisitRootVisitor("MyField", "long");
        visitor.Visit(rangeClause);
        return rangeClause.KustoQL;
    }

    private static string DateRangeClauseToKQL(RangeClause rangeClause)
    {
        var visitor = VisitorTestsUtils.CreateAndVisitRootVisitor("MyField", "date");
        visitor.Visit(rangeClause);
        return rangeClause.KustoQL;
    }

    private static string TextRangeClauseToKQL(RangeClause rangeClause)
    {
        var visitor = VisitorTestsUtils.CreateAndVisitRootVisitor("MyField", "keyword");
        visitor.Visit(rangeClause);
        return rangeClause.KustoQL;
    }
}
