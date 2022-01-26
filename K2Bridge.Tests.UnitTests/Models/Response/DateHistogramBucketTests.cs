// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace UnitTests.K2Bridge.Models.Response
{
    using System;
    using System.Data;
    using global::K2Bridge.Factories;
    using global::K2Bridge.Models;
    using Microsoft.Extensions.Logging;
    using Moq;
    using Newtonsoft.Json.Linq;
    using NUnit.Framework;

    [TestFixture]
    public class DateHistogramBucketTests
    {
        [Test]
        public void Create_WithDateTime_ReturnsISOString()
        {
            // Arrange
            string primaryKey = "timestamp";
            DataTable table = new DataTable();
            table.Columns.Add(primaryKey, typeof(DateTime)).DateTimeMode = DataSetDateTime.Utc;
            table.Columns.Add("count_", typeof(int));

            DataRow row = table.NewRow();
            row[primaryKey] = new DateTime(2017, 1, 2, 13, 4, 5, 60, DateTimeKind.Utc);
            row["count_"] = 234;

            QueryData data = new QueryData("query", "index");

            // Act
            var logger = Mock.Of<ILogger<dynamic>>();
            var bucket = BucketFactory.CreateDateHistogramBucket(primaryKey, row, data, logger);

            // Assert
            Assert.AreEqual("2017-01-02T13:04:05.060Z", bucket.KeyAsString);
            Assert.AreEqual(1483362245060, bucket.Key);
            Assert.AreEqual(234, bucket.DocCount);

            table.Dispose();
        }

        [Test]
        public void Create_WithDateTimeAndKeys_ReturnsISOString()
        {
            // Arrange
            string primaryKey = "timestamp";
            DataTable table = new DataTable();
            table.Columns.Add(primaryKey, typeof(DateTime)).DateTimeMode = DataSetDateTime.Utc;
            table.Columns.Add("count_", typeof(int));
            table.Columns.Add("1%percentile%50.0%True", typeof(JArray));

            DataRow row = table.NewRow();
            row[primaryKey] = new DateTime(2017, 1, 2, 13, 4, 5, 60, DateTimeKind.Utc);
            row["count_"] = 234;
            row["1%percentile%50.0%True"] = new JArray(644.54658);

            QueryData data = new QueryData("query", "index");

            // Act
            var logger = Mock.Of<ILogger<dynamic>>();
            var bucket = BucketFactory.CreateDateHistogramBucket(primaryKey, row, data, logger);

            // Assert
            Assert.AreEqual("2017-01-02T13:04:05.060Z", bucket.KeyAsString);
            Assert.AreEqual(1483362245060, bucket.Key);
            Assert.AreEqual(234, bucket.DocCount);
            Assert.AreEqual(1, bucket.Count);

            table.Dispose();
        }
    }
}
