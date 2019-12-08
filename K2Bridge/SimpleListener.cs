// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace K2Bridge
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Text;
    using Flurl;
    using K2Bridge.KustoConnector;
    using K2Bridge.Models;
    using K2Bridge.Models.Response;
    using K2Bridge.RequestHandlers;
    using Microsoft.Extensions.Logging;
    using Newtonsoft.Json;

    internal class SimpleListener
    {
        private readonly TraceHelper tracer;

        private readonly ILogger<SimpleListener> logger;

        private readonly string[] prefixes;

        private readonly string remoteEndpoint;

        private readonly bool isCompareResponses;

        private readonly bool isHandleMetadata;

        private readonly ITranslator translator;

        private readonly IQueryExecutor kustoManager;

        public SimpleListener(
            ListenerDetails listenerDetails,
            ITranslator queryTranslator,
            IQueryExecutor kustoManager,
            ILoggerFactory loggerFactory)
        {
            this.prefixes = listenerDetails.Prefixes;
            this.remoteEndpoint = listenerDetails.RemoteEndpoint;
            this.translator = queryTranslator;
            this.isCompareResponses = listenerDetails.IsCompareResponse;
            this.isHandleMetadata = listenerDetails.IsHandleMetadata;
            this.kustoManager = kustoManager;
            this.logger = loggerFactory.CreateLogger<SimpleListener>();
            this.tracer = new TraceHelper(this.logger, @"../../../Traces");
        }

        private enum RequestType
        {
            NA, // = not interesting for K2 to handle
            Data,
            Metadata,
        }

        public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(5);

        public void Start()
        {
            string requestId = string.Empty;

            if (!HttpListener.IsSupported)
            {
                this.logger.LogError("OS doesn't support using the HttpListener class.");
                return;
            }

            if (this.prefixes == null || this.prefixes.Length == 0)
            {
                throw new ArgumentException("URI prefixes are required, for example http://contoso.com:8080/index/");
            }

            // Create a listener.
            HttpListener listener = new HttpListener();

            // Add the prefixes.
            foreach (string s in this.prefixes)
            {
                listener.Prefixes.Add(s);
            }

            listener.Start();
            this.logger.LogInformation($"Proxy is Listening on {listener.Prefixes.Aggregate((s1, s2) => $"{s1};{s2}")}...");
            this.logger.LogInformation($"Remote elasticsearch is at {this.remoteEndpoint}");
            this.logger.LogInformation("Press Ctrl+C to exit.");

            while (true)
            {
                try
                {
                    // Note: The GetContext method blocks while waiting for a request.
                    HttpListenerContext context = listener.GetContext();
                    HttpListenerRequest request = context.Request;

                    // use a stream we can read more than once
                    var requestInputStream = new MemoryStream();
                    request.InputStream.CopyStream(requestInputStream);

                    bool isRequestTraceEnabled = false;
                    bool isRequestAnsweredSuccessfully = false;
                    RequestType requestType = RequestType.NA;

                    // Obtain a response object.
                    HttpListenerResponse response = context.Response;
                    requestId = request.RequestTraceIdentifier.ToString();
                    response.Headers.Add("X-K2-CorrelationId", requestId);

                    var sr = new StreamReader(requestInputStream);
                    var requestInputString = sr.ReadToEnd();
                    requestInputStream.Position = 0;

                    string responseString = null;
                    MemoryStream kustoResultsStream = null;

                    if (IndexListRequestHandler.CanAnswer(request.RawUrl, requestInputString))
                    {
                        isRequestTraceEnabled = true;
                        requestType = RequestType.Metadata;
                        this.tracer.WriteFile($"{requestId}.Request.json", requestInputStream);

                        var handler = new IndexListRequestHandler(context, this.kustoManager, requestId, this.logger);

                        responseString = handler.PrepareResponse(request.RawUrl);
                        if (this.isHandleMetadata)
                        {
                            isRequestAnsweredSuccessfully = true;
                        }
                    }
                    else if (FieldCapabilityRequestHandler.CanAnswer(request.RawUrl))
                    {
                        isRequestTraceEnabled = true;
                        requestType = RequestType.Metadata;
                        this.tracer.WriteFile($"{requestId}.Request.json", requestInputStream);

                        var handler = new FieldCapabilityRequestHandler(context, this.kustoManager, requestId, this.logger);

                        responseString = handler.PrepareResponse(request.RawUrl);

                        if (this.isHandleMetadata)
                        {
                            isRequestAnsweredSuccessfully = true;
                        }
                    }
                    else if (request.RawUrl.StartsWith(@"/_msearch"))
                    {
                        try
                        {
                            isRequestTraceEnabled = true;
                            requestType = RequestType.Data;
                            this.tracer.WriteFile($"{requestId}.Request.json", requestInputStream);

                            string body = this.GetRequestBody(request, requestInputStream);

                            // The body is in NDJson
                            string[] lines = body.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries);

                            // TODO: add ability to handle multiple queries
                            // https://dev.azure.com/csedevil/K2-bridge-internal/_backlogs/backlog/K2-bridge-internal%20Team/Stories/?workitem=1172
                            this.logger.LogDebug($"Elastic search request:\n{lines[0]}\n{lines[1]}");

                            var translatedResponse = this.translator.Translate(lines[0], lines[1]);
                            this.logger.LogDebug($"Translated query:\n{translatedResponse.KQL}");
                            this.tracer.WriteFile($"{requestId}.KQL.json", translatedResponse.KQL);

                            ElasticResponse kustoResults = this.kustoManager.ExecuteQuery(translatedResponse);
                            responseString = JsonConvert.SerializeObject(kustoResults);

                            isRequestAnsweredSuccessfully = true;
                        }
                        catch (Exception ex)
                        {
                            this.logger.LogError(ex, "Failed to translate query.");
                        }
                    }

                    MemoryStream remoteResponseStream = null;
                    HttpWebResponse remoteResponse = null;

                    if ((requestType == RequestType.NA) ||
                        (requestType == RequestType.Data && !isRequestAnsweredSuccessfully) ||
                        (requestType == RequestType.Metadata && !this.isHandleMetadata) ||
                        (requestType != RequestType.NA && this.isCompareResponses))
                    {
                        remoteResponse = this.PassThrough(request, this.Timeout, requestInputStream);

                        if (remoteResponse != null)
                        {
                            // use a stream we can read more than once
                            remoteResponseStream = new MemoryStream();
                            remoteResponse.GetResponseStream().CopyStream(remoteResponseStream);
                        }
                    }

                    if (isRequestAnsweredSuccessfully)
                    {
                        byte[] kustoResultsContent = Encoding.ASCII.GetBytes(responseString);

                        kustoResultsStream = new MemoryStream(kustoResultsContent);

                        if (isRequestTraceEnabled)
                        {
                            if (remoteResponseStream != null)
                            {
                                this.tracer.WriteFile($"{requestId}.ElasticResponse.json", remoteResponseStream);
                            }

                            if (kustoResultsStream != null)
                            {
                                kustoResultsStream.Position = 0;
                                this.tracer.WriteFile($"{request.RequestTraceIdentifier}.K2Response.json", kustoResultsStream);
                            }
                        }

                        response.StatusCode = (int)HttpStatusCode.OK;
                        response.ContentLength64 = kustoResultsContent.LongLength;
                        response.ContentType = "application/json";

                        kustoResultsStream.CopyStream(response.OutputStream);

                        response.OutputStream.Close();
                    }
                    else
                    {
                        // We didn't answer the request yet, so use the elastic pass-through response
                        response.StatusCode = (int)remoteResponse.StatusCode;
                        response.ContentLength64 = remoteResponse.ContentLength;
                        response.ContentType = remoteResponse.ContentType;
                        response.Headers.Add("X-K2-PassThrough", "true");

                        if (isRequestTraceEnabled)
                        {
                            this.logger.LogError("Something went wrong, returning passthrough (=elastic's) response");
                        }

                        // Send the respose back
                        var output = response.OutputStream;
                        remoteResponseStream.CopyStream(output);
                        output.Close();
                    }

                    if (isRequestTraceEnabled && this.isCompareResponses && remoteResponseStream != null)
                    {
                        ResponseComparer rc = new ResponseComparer(this.logger, "not yet", requestId);
                        rc.CompareStreams(remoteResponseStream, kustoResultsStream);
                    }
                }
                catch (Exception ex)
                {
                    this.logger.LogError(ex, $"{requestId}: An exception...");
                }
            }
        }

        private HttpWebResponse PassThrough(HttpListenerRequest request, TimeSpan timeout, MemoryStream memoryStream)
        {
            try
            {
                string[] bodylessMethods = { "GET", "HEAD" };

                var requestString = Url.Combine(this.remoteEndpoint, request.RawUrl);
                var remoteRequest = WebRequest.Create(requestString) as HttpWebRequest;
                remoteRequest.AllowAutoRedirect = false;
                remoteRequest.KeepAlive = request.KeepAlive;
                remoteRequest.Proxy.Credentials = CredentialCache.DefaultCredentials;
                remoteRequest.Method = request.HttpMethod;
                remoteRequest.ContentType = request.ContentType;

                var cookies = new CookieContainer();
                cookies.Add(new Uri(requestString), request.Cookies);
                remoteRequest.CookieContainer = cookies;
                remoteRequest.Timeout = (int)timeout.TotalMilliseconds;

                if (!bodylessMethods.Contains(remoteRequest.Method))
                {
                    remoteRequest.ContentLength = request.ContentLength64;

                    using (var stream = remoteRequest.GetRequestStream())
                    {
                        // This is a fallback since we already read the source stream
                        memoryStream.CopyStream(stream);
                    }
                }

                var remoteResponse = (HttpWebResponse)remoteRequest.GetResponse();

                return remoteResponse;
            }
            catch (WebException ex)
            {
                if (ex.Status == WebExceptionStatus.ProtocolError && ex.Response != null)
                {
                    var resp = (HttpWebResponse)ex.Response;
                    return resp;
                }

                this.logger.LogError(ex, $"PassThrough request to: {request.RawUrl} ended with an exception");
                throw;
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, $"PassThrough request to: {request.RawUrl} ended with an exception");
                throw;
            }
        }

        private string GetRequestBody(HttpListenerRequest request, MemoryStream bodyMemoryStream)
        {
            if (!request.HasEntityBody)
            {
                return null;
            }

            bodyMemoryStream.Position = 0;
            using (var reader = new StreamReader(bodyMemoryStream, request.ContentEncoding, false, 4096, true))
            {
                return reader.ReadToEnd();
            }
        }
    }
}
