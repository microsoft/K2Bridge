// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace UnitTests.K2Bridge.KustoDAL
{
    using System;
    using global::K2Bridge.KustoDAL;
    using NUnit.Framework;

    [TestFixture]
    public class ParseExceptionTests
    {
        [Test]
        public void Constructor_WithNoArgument_ThrowsArgumentException()
        {
            Assert.Throws<ArgumentException>(() => new ParseException());
        }

        [Test]
        public void Constructor_WithMessage_ThrowsArgumentException()
        {
            Assert.Throws<ArgumentException>(() => new ParseException("test"));
        }

        [Test]
        public void Constructor_WithInnerExceptionAndMessage_ConstructsParseException()
        {
            var exc = new ParseException("test", new ArgumentException("test"));
            Assert.AreEqual(ParseException.ParsePhaseName, exc.PhaseName);
        }
    }
}