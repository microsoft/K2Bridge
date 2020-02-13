// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace UnitTests.K2Bridge.Models.Response
{
    using System;
    using System.Data;
    using global::K2Bridge.Models.Response;
    using NUnit.Framework;

    [TestFixture]
    public class DateHistogramBucketTests
    {
        [Test]
        public void Create_WithDateTime_ReturnsISOString()
        {
            // Arrange
            DataTable table = new DataTable();
            table.Columns.Add("Timestamp", typeof(DateTime)).DateTimeMode = DataSetDateTime.Utc;
            table.Columns.Add("Count", typeof(int));
            DataRow row = table.NewRow();
            row["Timestamp"] = new DateTime(2017, 1, 2, 13, 4, 5, 60, DateTimeKind.Utc);
            row["Count"] = 234;

            // Act
            var bucket = DateHistogramBucket.Create(row);

            // Assert
            Assert.AreEqual("2017-01-02T13:04:05.060Z", bucket.KeyAsString);
            Assert.AreEqual(1483362245060, bucket.Key);
            Assert.AreEqual(234, bucket.DocCount);

            table.Dispose();
        }
    }
}
