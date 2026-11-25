// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2Bridge.Tests.UnitTests.Utils;

using System;
using System.Collections.Generic;
using K2Bridge.Utils;
using Kusto.Cloud.Platform.Text;
using NUnit.Framework;

[TestFixture]
public class StringExtensionsTests
{
    [TestCase("logs", ExpectedResult = "['logs']")]
    [TestCase("", ExpectedResult = "['']")]
    [TestCase("lo'gs\\", ExpectedResult = "['lo\\'gs\\\\']")]
    [TestCase("logs\\'] | count", ExpectedResult = "['logs\\\\\\'] | count']")]
    [TestCase("logs[archive]", ExpectedResult = "['logs[archive]']")]
    [TestCase("logs\\n", ExpectedResult = "['logs\\\\n']")]
    [TestCase("logs\n", ExpectedResult = "['logs\\u000a']")]
    public string QuoteKustoTable_WithSpecialCharacters_EncodesIdentifier(string table) => table.QuoteKustoTable();

    [TestCase("", ExpectedResult = "\"\"")]
    [TestCase("db'\"\\name", ExpectedResult = "\"db'\\\"\\\\name\"")]
    [TestCase("db\\\"name", ExpectedResult = "\"db\\\\\\\"name\"")]
    [TestCase("db\\u0022", ExpectedResult = "\"db\\\\u0022\"")]
    [TestCase("db\\\\", ExpectedResult = "\"db\\\\\\\\\"")]
    [TestCase("\0\a\b\t\n\v\f\r\u001b\u007f\u0085\u2028\u2029", ExpectedResult = "\"\\u0000\\u0007\\u0008\\u0009\\u000a\\u000b\\u000c\\u000d\\u001b\\u007f\\u0085\\u2028\\u2029\"")]
    public string ToKustoStringLiteral_WithSpecialCharacters_EncodesLiteral(string value) => value.ToKustoStringLiteral();

    [TestCaseSource(nameof(LiteralRoundTripTestCases))]
    public void EncodeKustoNames_PreservesValueWhenDecodedBySdk(string value)
    {
        var literal = value.ToKustoStringLiteral();
        var identifier = value.QuoteKustoTable();

        Assert.Multiple(() =>
        {
            Assert.That(literal, Does.StartWith("\"").And.EndWith("\""));
            Assert.That(identifier, Does.StartWith("['").And.EndWith("']"));
            Assert.That(StringLiteral.ParseStringLiteral(literal), Is.EqualTo(value));
            Assert.That(StringLiteral.ParseStringLiteral(identifier[1..^1]), Is.EqualTo(value));
        });
    }

    [Test]
    public void EncodeKustoNames_WithNull_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => StringExtensions.QuoteKustoTable(null));
        Assert.Throws<ArgumentNullException>(() => StringExtensions.ToKustoStringLiteral(null));
    }

    private static IEnumerable<TestCaseData> LiteralRoundTripTestCases()
    {
        var names = new[]
        {
            string.Empty,
            "logs",
            "where",
            "123logs",
            "prod-db.logs_2026",
            " logs archive ",
            "logs:archive",
            "*logs*",
            "['logs']",
            "lo'\"gs\\",
            "\\n\\r\\t\\u0027\\u0022",
            "\0\a\b\t\n\v\f\r\u001b\u007f\u0085\u2028\u2029",
            "\u6570\u636e_\u00e9_\ud83d\ude00",
        };
        for (var index = 0; index < names.Length; index++)
        {
            yield return new TestCaseData(names[index])
                .SetName($"EncodeKustoNames_SdkRoundTrip_Name_{index}");
        }

        var characters = new[] { 'a', '\\', '\'', '"', '[', ']', '\n', '\0', '\u2028', '\u2029' };
        foreach (var first in characters)
        {
            foreach (var second in characters)
            {
                yield return new TestCaseData(new string(new[] { first, second }))
                    .SetName($"EncodeKustoNames_SdkRoundTrip_Pair_{(int)first:x4}_{(int)second:x4}");
            }
        }
    }
}
