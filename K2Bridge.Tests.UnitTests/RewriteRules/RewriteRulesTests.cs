// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace UnitTests.K2Bridge.RewriteRules
{
    using global::K2Bridge.RewriteRules;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Rewrite;
    using Moq;
    using NUnit.Framework;

    [TestFixture]
    public class RewriteRulesTests
    {
        private static readonly object[] RewriteMissingTrailsPath = {
            new TestCaseData("/test/validfile.html").Returns("/test/validfile.html").SetName("TrailSlashRewriteRules_WhenValidFile_DoNotAddSlash"),
            new TestCaseData("/test/.html").Returns("/test/.html/").SetName("TrailSlashRewriteRules_EmptyFileName_AddSlash"),
            new TestCaseData("/test/.html/").Returns("/test/.html/").SetName("TrailSlashRewriteRules_ContainsTrailingSlash_LeaveTrailingSlashes"),
            new TestCaseData("/a").Returns("/a/").SetName("TrailSlashRewriteRules_NoTrailingSlash_AddSlash"),
            new TestCaseData(string.Empty).Returns(string.Empty).SetName("TrailSlashRewriteRules_EmptyString_NoError"),
            new TestCaseData("/").Returns("/").SetName("TrailSlashRewriteRules_OnlySlash_NoError"),
            new TestCaseData("/./.").Returns("/././").SetName("TrailSlashRewriteRules_MultipleSlashesAndDot_AddsSlash"),
            new TestCaseData("/./a.").Returns("/./a./").SetName("TrailSlashRewriteRules_MultipleSlashesAndDotAndCharacter_AddsSlash"),
        };

        private static readonly object[] RewriteFieldCapsPath = {
            new TestCaseData("/myindex/_field_caps").Returns("/FieldCapability/Process/myindex").SetName("FieldCapsRewriteRules_ValidInput_SetFieldCaps"),
            new TestCaseData("/*/_field_caps").Returns("/FieldCapability/Process/*").SetName("FieldCapsRewriteRules_StarValidInput_SetFieldCapsStar"),
            new TestCaseData(string.Empty).Returns(string.Empty).SetName("FieldCapsRewriteRules_EmptyString_NoError"),
            new TestCaseData("/not_parsed").Returns("/not_parsed").SetName("FieldCapsRewriteRules_OtherInput_DoesNotHandle"),
            new TestCaseData("/_field_caps").Returns("/FieldCapability/Process/_field_caps").SetName("FieldCapsRewriteRules_FieldCaps_NoError"),
        };

        private static readonly object[] RewriteIndexListPath = {
            new TestCaseData("/myindex/_search").Returns("/IndexList/Process/myindex").SetName("IndexListRewriteRule_ValidInput_SetIndexList"),
            new TestCaseData("/.kibana/_search").Returns("/.kibana/_search").SetName("IndexListRewriteRule_KibanaInternal_NotChanged"),
            new TestCaseData("/myindex/notsearch").Returns("/myindex/notsearch").SetName("IndexListRewriteRule_OtherInput_NotChanged"),
            new TestCaseData("/_search").Returns("/IndexList/Process/_search").SetName("IndexListRewriteRule_EmptySearch_NoError"),
        };

        private static readonly object[] RewriteRequestForTemplatePath = {
            new TestCaseData("/_template/myindex:.someindex/").Returns("/_template/myindex::someindex/").SetName("TemplateRewriteRules_ValidInput_SetTemplate"),
            new TestCaseData("/_template/myindex/").Returns("/_template/myindex/").SetName("TemplateRewriteRules_NoIllegalChars_NotChanged"),
            new TestCaseData("/nottemplate/myindex:.").Returns("/nottemplate/myindex:.").SetName("TemplateRewriteRules_NotTemplate_NotChanged"),
            new TestCaseData("/nottemplatetemplate/myindex/").Returns("/nottemplatetemplate/myindex/").SetName("TemplateRewriteRules_NoIllegalCharAndNotTemplate_NotChanged"),
        };

        [TestCaseSource(nameof(RewriteMissingTrailsPath))]
        public string TrailingSlashRulesTest(string request)
        {
            return TestIRule(new RewriteTrailingSlashesRule(), request);
        }

        [TestCaseSource(nameof(RewriteFieldCapsPath))]
        public string RewriteFieldCapsTest(string request)
        {
            return TestIRule(new RewriteFieldCapabilitiesRule(), request);
        }

        [TestCaseSource(nameof(RewriteIndexListPath))]
        public string RewriteIndexListsTest(string request)
        {
            return TestIRule(new RewriteIndexListRule(), request);
        }

        [TestCaseSource(nameof(RewriteRequestForTemplatePath))]
        public string RewriteRequestForTemplateTest(string request)
        {
            return TestIRule(new RewriteRequestsForTemplateRule(), request);
        }

        private static string TestIRule(IRule rule, string request)
        {
            // Arrange
            var context = MakeContext(request);

            // Act
            rule.ApplyRule(context);

            // Assert
            return context.HttpContext.Request.Path.ToString();
        }

        private static RewriteContext MakeContext(string requestString)
        {
            var clusureRequestString = requestString;
            var request = new Mock<HttpRequest>();
            var httpContext = new Mock<HttpContext>();
            request.Setup(x => x.Path).Returns(() => new PathString(clusureRequestString));
            request.SetupSet(x => x.Path = It.IsAny<PathString>()).Callback<PathString>(p => clusureRequestString = p.Value);
            httpContext.Setup(x => x.Request).Returns(request.Object);
            return new RewriteContext() { HttpContext = httpContext.Object };
        }
    }
}
