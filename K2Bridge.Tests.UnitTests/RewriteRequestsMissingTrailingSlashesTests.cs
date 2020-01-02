// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2BridgeUnitTests
{
    using K2Bridge.RewriteRules;
    using Microsoft.AspNetCore.Http;
    using NUnit.Framework;

    [TestFixture]
    public class RewriteRequestsMissingTrailingSlashesTests
    {
        [Test]
        public void TrailingSlashRewrite_DoNotAddSlashWhenRequestingFiles()
        {
            // Arrange
            var httpRequestPath = new PathString("/test/validfile.html");

            // Act
            var uat = new RewriteTrailingSlashesRule();
            var result = uat.RewritePath(httpRequestPath);

            // Assert
            Assert.AreEqual(result.ToString(), httpRequestPath.ToString());
        }

        [Test]
        public void TrailingSlashRewrite_AddSlashWhenRequestingPath()
        {
            // Arrange
            var httpRequestPath = new PathString("/test/.html");

            // Act
            var uat = new RewriteTrailingSlashesRule();
            var result = uat.RewritePath(httpRequestPath);

            // Assert
            Assert.AreEqual(result.ToString(), httpRequestPath + '/');
        }

        [Test]
        public void TrailingSlashRewrite_DoNotAddSlashWhenAlreadyThere()
        {
            // Arrange
            var httpRequestPath = new PathString("/test/.html/");

            // Act
            var uat = new RewriteTrailingSlashesRule();
            var result = uat.RewritePath(httpRequestPath);

            // Assert
            Assert.AreEqual(result.ToString(), httpRequestPath.ToString());
        }
    }
}
