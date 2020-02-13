// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace UnitTests.K2Bridge.JsonConverters
{
    using System;
    using global::K2Bridge.JsonConverters;
    using NUnit.Framework;

    [TestFixture]
    public partial class JsonConvertersTests
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
}