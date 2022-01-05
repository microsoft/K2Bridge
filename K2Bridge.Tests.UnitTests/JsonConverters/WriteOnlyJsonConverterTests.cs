// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace UnitTests.K2Bridge.JsonConverters
{
    using System;
    using global::K2Bridge.JsonConverters;
    using NUnit.Framework;

    [TestFixture]
    public class WriteOnlyJsonConverterTests
    {
        [Test]
        public void WriteOnlyJsonConverter_CanRead_ReturnsFalse()
        {
            var writeOnlyConverter = new PercentileAggregateConverter();
            Assert.IsFalse(writeOnlyConverter.CanRead);
        }

        [Test]
        public void WriteOnlyJsonConverter_NotImplementedMethods_ThrowsAsExpected()
        {
            var writeOnlyConverter = new PercentileAggregateConverter();
            Assert.Throws<NotImplementedException>(() => writeOnlyConverter.CanConvert(typeof(string)));
            Assert.Throws<NotImplementedException>(() => writeOnlyConverter.ReadJson(null, null, null, null));
        }
    }
}