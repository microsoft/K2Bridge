// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace UnitTests.K2Bridge.KustoDAL
{
    using System;
    using global::K2Bridge;
    using NUnit.Framework;

    [TestFixture]
    public class TranslateExceptionTests
    {
        [Test]
        public void Constructor_WithNoArgument_ThrowsArgumentException()
        {
            Assert.Throws<ArgumentException>(() => new TranslateException());
        }

        [Test]
        public void Constructor_WithMessage_ThrowsArgumentException()
        {
            Assert.Throws<ArgumentException>(() => new TranslateException("test"));
        }

        [Test]
        public void Constructor_WithInnerExceptionAndMessage_ConstructsTranslateException()
        {
            var exc = new TranslateException("test", new ArgumentException("test"));
            Assert.AreEqual(TranslateException.TranslatePhaseName, exc.PhaseName);
        }
    }
}