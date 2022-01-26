// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace UnitTests.K2Bridge.Models
{
    using System;
    using global::K2Bridge.Factories;
    using Microsoft.Extensions.Configuration;
    using Moq;
    using NUnit.Framework;

    [TestFixture]
    public class MetadataConnectionDetailsTests
    {
        [TestCase]
        public void MakeFromConfig_WithValidValues_ReturnsValidConnectionDetails()
        {
            var configurationRoot = new Mock<IConfigurationRoot>();
            var contosoesAddress = "http://contoso-es:1234/";
            configurationRoot.SetupGet(
                x => x[It.Is<string>(s => s.Equals("metadataElasticAddress", StringComparison.OrdinalIgnoreCase))]).Returns(contosoesAddress);

            var metadataConnectionDetails = MetadataConnectionDetailsFactory.MakeFromConfiguration(configurationRoot.Object);
            Assert.NotNull(metadataConnectionDetails);
            Assert.AreEqual(contosoesAddress, metadataConnectionDetails.MetadataEndpoint);
        }

        [TestCase]
        public void MakeFromConfig_WithMissingAddress_ThrowsArgumentNullException()
        {
            var configurationRoot = new Mock<IConfigurationRoot>();

            // missing 'metadataElasticAddress'
            Assert.That(() => MetadataConnectionDetailsFactory.MakeFromConfiguration(configurationRoot.Object), Throws.TypeOf<ArgumentNullException>());
        }
    }
}
