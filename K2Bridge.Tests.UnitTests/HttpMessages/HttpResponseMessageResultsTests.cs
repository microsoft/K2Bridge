// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

using System;
using System.Net.Http;
using System.Threading.Tasks;
using K2Bridge.HttpMessages;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using NUnit.Framework;

namespace K2Bridge.Tests.UnitTests.HttpMessages;

[System.Diagnostics.CodeAnalysis.SuppressMessage("Reliability", "CA1001:owns disposable field(s) but is not disposable", Justification = "No need to do this.")]
[TestFixture]
public class HttpResponseMessageResultsTests
{
    private const string Reason = "A good reason";

    private HttpResponseMessageResult httpResponseMessageResult;

    private HttpResponseMessage httpResponseMessage;

    [Test]
    public void Ctor_InvalidArg_ThrowsArgumentNullException()
    {
        Assert.Throws(
            Is.TypeOf<ArgumentNullException>()
             .And.Message.Contains("Value cannot be null."),
            () => new HttpResponseMessageResult(null));
    }

    [SetUp]
    public void SetUp()
    {
        httpResponseMessage = new HttpResponseMessage
        {
            StatusCode = (System.Net.HttpStatusCode)200,
            ReasonPhrase = Reason,
        };

        httpResponseMessage.Headers.Add("my-custom-header", "val 1");

        httpResponseMessage.Content = new StringContent("Your response text");
        httpResponseMessage.Content.Headers.Add("my-custom-content-header", "val 1");

        httpResponseMessageResult = new HttpResponseMessageResult(httpResponseMessage);
    }

    [Test]
    public async Task ExecuteResultAsync_WithValidContext_Passes()
    {
        // The ActionContext will hold the copy result
        var ac = new Microsoft.AspNetCore.Mvc.ActionContext
        {
            HttpContext = new DefaultHttpContext(),
        };

        // Execute
        await httpResponseMessageResult.ExecuteResultAsync(ac);

        var res = ac.HttpContext.Response;
        Assert.AreEqual(200, res.StatusCode);

        var responseFeature = ac.HttpContext.Features.Get<IHttpResponseFeature>();
        Assert.AreEqual(Reason, responseFeature.ReasonPhrase);

        res.Headers.TryGetValue("my-custom-header", out var headerVal);
        Assert.AreEqual(httpResponseMessage.Headers.GetValues("my-custom-header"), headerVal);

        var val = httpResponseMessage.Content.Headers.GetValues("my-custom-content-header");
        res.Headers.TryGetValue("my-custom-content-header", out var contentHeaderVal);
        Assert.AreEqual(val, contentHeaderVal);
    }

    [Test]
    public async Task ExecuteResultAsync_IgnoresTransferEncoding_Passes()
    {
        // The ActionContext will hold the copy result
        var ac = new Microsoft.AspNetCore.Mvc.ActionContext
        {
            HttpContext = new DefaultHttpContext(),
        };

        // Set transfer encoding
        httpResponseMessage.Headers.TransferEncodingChunked = true;

        // recreate result object with updated message.
        httpResponseMessageResult = new HttpResponseMessageResult(httpResponseMessage);

        // Execute
        await httpResponseMessageResult.ExecuteResultAsync(ac);

        var res = ac.HttpContext.Response;
        Assert.AreEqual(200, res.StatusCode);

        var responseFeature = ac.HttpContext.Features.Get<IHttpResponseFeature>();
        Assert.AreEqual(Reason, responseFeature.ReasonPhrase);

        res.Headers.TryGetValue("my-custom-header", out var headerVal);
        Assert.AreEqual(httpResponseMessage.Headers.GetValues("my-custom-header"), headerVal);

        var val = httpResponseMessage.Content.Headers.GetValues("my-custom-content-header");
        res.Headers.TryGetValue("my-custom-content-header", out var contentHeaderVal);
        Assert.AreEqual(val, contentHeaderVal);

        // Verify transfer encoding was ignored
        res.Headers.TryGetValue("Transfer-Encoding", out var transferEnc);
        Assert.IsTrue(transferEnc.Count == 0);
    }

    [Test]
    public void ExecuteResultAsync_WithActionContextNoHttpContext_Throws()
    {
        // The ActionContext that SHOULD hold the copy result
        var ac = new Microsoft.AspNetCore.Mvc.ActionContext();

        // Execute
        Assert.ThrowsAsync(
            Is.TypeOf<ArgumentNullException>()
             .And.Message.EqualTo($"Response message can not be null (Parameter 'response')"),
            async () => await httpResponseMessageResult.ExecuteResultAsync(ac));
    }

    [Test]
    public void ExecuteResultAsync_WithoutActionContext_Throws()
    {
        // Execute
        Assert.ThrowsAsync(
            Is.TypeOf<ArgumentNullException>()
             .And.Message.EqualTo($"Response message can not be null (Parameter 'response')"),
            async () => await httpResponseMessageResult.ExecuteResultAsync(null));
    }
}
