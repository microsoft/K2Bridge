// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2Bridge.Tests.UnitTests.Models.Response.Metadata
{
    using System;
    using System.Data;
    using K2Bridge.Models.Response.Metadata;
    using Moq;
    using NUnit.Framework;

    [TestFixture]
    public class FieldCapabilityElementTests
    {
        private Mock<IDataRecord> stubDataRecord;

        [SetUp]
        public void SetUp()
        {
            stubDataRecord = new Mock<IDataRecord>();
        }

        [Test]
        public void Create_WithInt32_ReturnsInteger()
        {
            stubDataRecord.Setup(r => r[0]).Returns("intColumnName");
            stubDataRecord.Setup(r => r[1]).Returns("System.Int32");

            var fieldCapabilityElement = FieldCapabilityElement.Create(stubDataRecord.Object);
            Assert.AreEqual(fieldCapabilityElement.Name, "intColumnName");
            Assert.AreEqual(fieldCapabilityElement.Type, "integer");
        }

        [Test]
        public void Create_WithInt64_ReturnsLong()
        {
            stubDataRecord.Setup(r => r[0]).Returns("longColumnName");
            stubDataRecord.Setup(r => r[1]).Returns("System.Int64");

            var fieldCapabilityElement = FieldCapabilityElement.Create(stubDataRecord.Object);
            Assert.AreEqual(fieldCapabilityElement.Name, "longColumnName");
            Assert.AreEqual(fieldCapabilityElement.Type, "long");
        }

        [Test]
        public void Create_WithSingle_ReturnsFloat()
        {
            stubDataRecord.Setup(r => r[0]).Returns("floatColumnName");
            stubDataRecord.Setup(r => r[1]).Returns("System.Single");

            var fieldCapabilityElement = FieldCapabilityElement.Create(stubDataRecord.Object);
            Assert.AreEqual(fieldCapabilityElement.Name, "floatColumnName");
            Assert.AreEqual(fieldCapabilityElement.Type, "float");
        }

        [Test]
        public void Create_WithDouble_ReturnsDouble()
        {
            stubDataRecord.Setup(r => r[0]).Returns("doubleColumnName");
            stubDataRecord.Setup(r => r[1]).Returns("System.Double");

            var fieldCapabilityElement = FieldCapabilityElement.Create(stubDataRecord.Object);
            Assert.AreEqual(fieldCapabilityElement.Name, "doubleColumnName");
            Assert.AreEqual(fieldCapabilityElement.Type, "double");
        }

        [Test]
        public void Create_WithSByte_ReturnsBoolean()
        {
            stubDataRecord.Setup(r => r[0]).Returns("boolColumnName");
            stubDataRecord.Setup(r => r[1]).Returns("System.SByte");

            var fieldCapabilityElement = FieldCapabilityElement.Create(stubDataRecord.Object);
            Assert.AreEqual(fieldCapabilityElement.Name, "boolColumnName");
            Assert.AreEqual(fieldCapabilityElement.Type, "boolean");
        }

        [Test]
        public void Create_WithObject_ReturnsObject()
        {
            stubDataRecord.Setup(r => r[0]).Returns("objectColumnName");
            stubDataRecord.Setup(r => r[1]).Returns("System.Object");

            var fieldCapabilityElement = FieldCapabilityElement.Create(stubDataRecord.Object);
            Assert.AreEqual(fieldCapabilityElement.Name, "objectColumnName");
            Assert.AreEqual(fieldCapabilityElement.Type, "object");
        }

        [Test]
        public void Create_WithString_ReturnsKeyword()
        {
            stubDataRecord.Setup(r => r[0]).Returns("stringColumnName");
            stubDataRecord.Setup(r => r[1]).Returns("System.String");

            var fieldCapabilityElement = FieldCapabilityElement.Create(stubDataRecord.Object);
            Assert.AreEqual(fieldCapabilityElement.Name, "stringColumnName");
            Assert.AreEqual(fieldCapabilityElement.Type, "keyword");
        }

        [Test]
        public void Create_WithDateTime_ReturnsDate()
        {
            stubDataRecord.Setup(r => r[0]).Returns("dateColumnName");
            stubDataRecord.Setup(r => r[1]).Returns("System.DateTime");

            var fieldCapabilityElement = FieldCapabilityElement.Create(stubDataRecord.Object);
            Assert.AreEqual(fieldCapabilityElement.Name, "dateColumnName");
            Assert.AreEqual(fieldCapabilityElement.Type, "date");
        }

        [Test]
        public void Create_WithSqlDecimal_ReturnsDouble()
        {
            stubDataRecord.Setup(r => r[0]).Returns("doubleColumnName");
            stubDataRecord.Setup(r => r[1]).Returns("System.Data.SqlTypes.SqlDecimal");

            var fieldCapabilityElement = FieldCapabilityElement.Create(stubDataRecord.Object);
            Assert.AreEqual(fieldCapabilityElement.Name, "doubleColumnName");
            Assert.AreEqual(fieldCapabilityElement.Type, "double");
        }

        [Test]
        public void Create_WithGuid_ReturnsString()
        {
            stubDataRecord.Setup(r => r[0]).Returns("guidColumnName");
            stubDataRecord.Setup(r => r[1]).Returns("System.Guid");

            var fieldCapabilityElement = FieldCapabilityElement.Create(stubDataRecord.Object);
            Assert.AreEqual(fieldCapabilityElement.Name, "guidColumnName");
            Assert.AreEqual(fieldCapabilityElement.Type, "string");
        }

        [Test]
        public void Create_WithTimespan_ReturnsString()
        {
            stubDataRecord.Setup(r => r[0]).Returns("timespanColumnName");
            stubDataRecord.Setup(r => r[1]).Returns("System.TimeSpan");

            var fieldCapabilityElement = FieldCapabilityElement.Create(stubDataRecord.Object);
            Assert.AreEqual(fieldCapabilityElement.Name, "timespanColumnName");
            Assert.AreEqual(fieldCapabilityElement.Type, "string");
        }

        [Test]
        public void Create_WithUnknownType_ThrowsArgumentException()
        {
            var dataType = "Unknown.Data.Type";
            stubDataRecord.Setup(r => r[0]).Returns("someColumnName");
            stubDataRecord.Setup(r => r[1]).Returns(dataType);

            Assert.Throws(
                Is.TypeOf<ArgumentException>()
                 .And.Message.EqualTo($"Kusto Type {dataType} does not map to a known ElasticSearch type"),
                () => FieldCapabilityElement.Create(stubDataRecord.Object));
        }
    }
}
