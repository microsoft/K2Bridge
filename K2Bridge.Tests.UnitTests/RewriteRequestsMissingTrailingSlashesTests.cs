using K2Bridge.RewriteRules;
using Microsoft.AspNetCore.Http;
using NUnit.Framework;

namespace K2BridgeUnitTests
{
    [TestFixture]
    public class RewriteRequestsMissingTrailingSlashesTests
    {
        [Test]
        public void TrailingSlashRewrite_DoNotAddSlashWhenRequestingFiles()
        {
            // Arrange
            var httpRequestPath = new PathString("/test/validfile.html");

            // Act
            var uat = new RewriteRequestsMissingTrailingSlashesRule();
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
            var uat = new RewriteRequestsMissingTrailingSlashesRule();
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
            var uat = new RewriteRequestsMissingTrailingSlashesRule();
            var result = uat.RewritePath(httpRequestPath);

            // Assert
            Assert.AreEqual(result.ToString(), httpRequestPath.ToString());
        }
    }
}
