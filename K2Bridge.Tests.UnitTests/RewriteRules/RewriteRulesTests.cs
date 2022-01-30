// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

using System.IO;
using System.Text;
using K2Bridge.RewriteRules;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Rewrite;
using Moq;
using NUnit.Framework;

namespace K2Bridge.Tests.UnitTests.RewriteRules;

[TestFixture]
public class RewriteRulesTests
{
    private const string SearchBodyForIndicies = "{\"size\":0,\"aggs\":{\"indices\":{\"terms\":{\"field\":\"_index\",\"size\":200}}}}";

    private static readonly object[] RewriteMissingTrailsPath = {
            new TestCaseData("/test/validfile.html").Returns("/test/validfile.html").SetName("RewriteTrailingSlashesRule_WhenValidFile_DoNotAddSlash"),
            new TestCaseData("/test/.html").Returns("/test/.html/").SetName("RewriteTrailingSlashesRule_EmptyFileName_AddSlash"),
            new TestCaseData("/test/.html/").Returns("/test/.html/").SetName("RewriteTrailingSlashesRule_ContainsTrailingSlash_LeaveTrailingSlashes"),
            new TestCaseData("/a").Returns("/a/").SetName("RewriteTrailingSlashesRule_NoTrailingSlash_AddSlash"),
            new TestCaseData(string.Empty).Returns(string.Empty).SetName("RewriteTrailingSlashesRule_EmptyString_NoError"),
            new TestCaseData("/").Returns("/").SetName("RewriteTrailingSlashesRule_OnlySlash_NoError"),
            new TestCaseData("/./.").Returns("/././").SetName("RewriteTrailingSlashesRule_MultipleSlashesAndDot_AddsSlash"),
            new TestCaseData("/./a.").Returns("/./a./").SetName("RewriteTrailingSlashesRule_MultipleSlashesAndDotAndCharacter_AddsSlash"),
        };

    private static readonly object[] RewriteFieldCapsPath = {
            new TestCaseData("/myindex/_field_caps").Returns("/FieldCapability/Process/myindex").SetName("RewriteFieldCapabilitiesRule_ValidInput_SetFieldCaps"),
            new TestCaseData("/*/_field_caps").Returns("/FieldCapability/Process/*").SetName("RewriteFieldCapabilitiesRule_StarValidInput_SetFieldCapsStar"),
            new TestCaseData(string.Empty).Returns(string.Empty).SetName("RewriteFieldCapabilitiesRule_EmptyString_NoError"),
            new TestCaseData("/not_parsed").Returns("/not_parsed").SetName("RewriteFieldCapabilitiesRule_OtherInput_DoesNotHandle"),
            new TestCaseData("/_field_caps").Returns("/FieldCapability/Process/_field_caps").SetName("RewriteFieldCapabilitiesRule_FieldCaps_NoError"),
        };

    private static readonly object[] RewriteSearchPath = {
            new TestCaseData("/.kibana/_search", SearchBodyForIndicies).Returns("/.kibana/_search").SetName("RewriteSearchRule_KibanaInternal_NotChanged"),
            new TestCaseData("/myindex/notsearch", SearchBodyForIndicies).Returns("/myindex/notsearch").SetName("RewriteSearchRule_OtherInput_NotChanged"),
            new TestCaseData("/myindex/_search", "{}").Returns("/Query/SingleSearch/myindex").SetName("RewriteSearchRule_SearchEmptyJsonBody_RerouteToSearch"),
            new TestCaseData("/myindex/_search", string.Empty).Returns("/Query/SingleSearch/myindex").SetName("RewriteSearchRule_SearchEmptyBody_RerouteToSearch"),
        };

    private static readonly object[] RewriteRequestForTemplatePath = {
            new TestCaseData("/_template/myindex:.someindex/").Returns("/_template/myindex::someindex/").SetName("RewriteRequestsForTemplateRule_ValidInput_SetTemplate"),
            new TestCaseData("/_template/myindex/").Returns("/_template/myindex/").SetName("RewriteRequestsForTemplateRule_NoIllegalChars_NotChanged"),
            new TestCaseData("/nottemplate/myindex:.").Returns("/nottemplate/myindex:.").SetName("RewriteRequestsForTemplateRule_NotTemplate_NotChanged"),
            new TestCaseData("/nottemplatetemplate/myindex/").Returns("/nottemplatetemplate/myindex/").SetName("RewriteRequestsForTemplateRule_NoIllegalCharAndNotTemplate_NotChanged"),
        };

    [TestCaseSource(nameof(RewriteMissingTrailsPath))]
    public string TrailingSlashRulesTest(string requestPath)
    {
        return TestIRule(new RewriteTrailingSlashesRule(), requestPath);
    }

    [TestCaseSource(nameof(RewriteFieldCapsPath))]
    public string RewriteFieldCapsTest(string requestPath)
    {
        return TestIRule(new RewriteFieldCapabilitiesRule(), requestPath);
    }

    [TestCaseSource(nameof(RewriteSearchPath))]
    public string RewriteSearchRuleTest(string requestPath, string requestBody)
    {
        return TestIRule(new RewriteSearchRule(), requestPath, requestBody);
    }

    [TestCaseSource(nameof(RewriteRequestForTemplatePath))]
    public string RewriteRequestForTemplateTest(string requestPath)
    {
        return TestIRule(new RewriteRequestsForTemplateRule(), requestPath);
    }

    private static string TestIRule(IRule rule, string requestPath, string requestBody = null)
    {
        // Arrange
        var context = MakeContext(requestPath, requestBody);

        // Act
        rule.ApplyRule(context);

        // Assert
        return context.HttpContext.Request.Path.ToString();
    }

    private static RewriteContext MakeContext(string requestPath, string requestBody)
    {
        var request = new Mock<HttpRequest>();
        request.Setup(x => x.Path).Returns(() => new PathString(requestPath));
        request.SetupSet(x => x.Path = It.IsAny<PathString>()).Callback<PathString>(p => requestPath = p.Value);
        request.Setup(x => x.Body).Returns(() => new MemoryStream(Encoding.UTF8.GetBytes(requestBody)));

        var httpContext = new Mock<HttpContext>();
        httpContext.Setup(x => x.Request).Returns(request.Object);

        return new RewriteContext() { HttpContext = httpContext.Object };
    }
}
