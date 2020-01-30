// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2Bridge.Tests.UnitTests
{
    using System;
    using K2Bridge.Models;
    using Microsoft.Extensions.Configuration;
    using Moq;
    using NUnit.Framework;

    [TestFixture]
    public class MetadataConnectionDetailsTests
    {
        [TestCase]
        public void MakeFromConfig_ValidValues_Succeed()
        {
            var configurationRoot = new Mock<IConfigurationRoot>();
            var contosoesAddress = "http://contoso-es:1234/";
            configurationRoot.SetupGet(
                x => x[It.Is<string>(s => s.Equals("metadataElasticAddress", StringComparison.InvariantCulture))]).Returns(contosoesAddress);

            var metadataConnectionDetails = MetadataConnectionDetails.MakeFromConfiguration(configurationRoot.Object);
            Assert.NotNull(metadataConnectionDetails);
            Assert.AreEqual(contosoesAddress, metadataConnectionDetails.MetadataEndpoint);
        }

        [TestCase]
        public void MakeFromConfig_MissingAddress_Fails()
        {
            var configurationRoot = new Mock<IConfigurationRoot>();

            // missing 'metadataElasticAddress'
            Assert.That(() => MetadataConnectionDetails.MakeFromConfiguration(configurationRoot.Object), Throws.TypeOf<ArgumentNullException>());
        }
    }
}
