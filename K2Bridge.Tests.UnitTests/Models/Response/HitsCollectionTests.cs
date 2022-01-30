// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2Bridge.Tests.UnitTests.Models.Response
{
    using K2Bridge.Models.Response;
    using NUnit.Framework;

    [TestFixture]
    public class HitsCollectionTests
    {
        [Test]
        public void HitsCollection_WhenAddToTotal_TotalCountIsUpdated()
        {
            var hitsCollection = new HitsCollection();

            hitsCollection.AddToTotal(17);

            Assert.AreEqual(17, hitsCollection.Total.Value);
        }

        [Test]
        public void HitsCollection_WhenAddedTwiceMaxIntValue_TotalCountIsUpdatedCorrectly()
        {
            var hitsCollection = new HitsCollection();

            hitsCollection.AddToTotal(int.MaxValue);
            hitsCollection.AddToTotal(int.MaxValue);

            Assert.That(hitsCollection.Total.Value > int.MaxValue);
        }
    }
}
