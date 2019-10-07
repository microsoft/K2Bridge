namespace K2Bridge
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Text;
    using K2Bridge.KustoConnector;
    using Microsoft.Extensions.Logging;
    using Newtonsoft.Json;

    internal class SimpleListener
    {
        private readonly TraceHelper tracer;

        private readonly ILogger<SimpleListener> logger;

        private readonly string[] prefixes;

        private readonly string remoteEndpoint;

        private readonly ITranslator translator;

        private readonly IQueryExecutor kustoManager;

        public SimpleListener(
            ListenerEndpointsDetails listenerEndpoints,
            ITranslator queryTranslator,
            IQueryExecutor kustoManager,
            ILoggerFactory loggerFactory)
        {
            this.prefixes = listenerEndpoints.Prefixes;
            this.remoteEndpoint = listenerEndpoints.RemoteEndpoint;
            this.translator = queryTranslator;
            this.kustoManager = kustoManager;
            this.logger = loggerFactory.CreateLogger<SimpleListener>();
            this.tracer = new TraceHelper(this.logger, @"../../../Traces");
        }

        public int TimeoutInMilliSeconds { get; set; } = 5000;

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
            this.logger.LogInformation("Proxy is Listening...");
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

                    bool requestTraceIsOn = false;
                    bool requestAnsweredSuccessfully = false;

                    // Obtain a response object.
                    HttpListenerResponse response = context.Response;
                    requestId = request.RequestTraceIdentifier.ToString();
                    response.Headers.Add("X-K2-CorrelationId", requestId);

                    if (request.RawUrl.StartsWith(@"/.reporting-*/esqueue/_search") &&
                        request.RawUrl.StartsWith(@"/.kibana_task_manager/_doc/_search"))
                    {
                        this.logger.LogDebug($"Request: {requestId}:{request.RawUrl}");
                    }

                    var sr = new StreamReader(requestInputStream);
                    string requestInputString = null;
                    requestInputString = sr.ReadToEnd();
                    requestInputStream.Position = 0;

                    if (requestTraceIsOn)
                    {
                        // Write the request before anything bad might happen.
                        this.tracer.WriteFile($"{requestId}.Request.json", requestInputStream);
                    }

                    string responseString = null;
                    MemoryStream kustoResultsStream = null;

                    if (request.RawUrl.StartsWith(@"/.kibana/"))
                    {
                        if (IndexListRequestHandler.Mine(request.RawUrl, requestInputString))
                        {
                            this.logger.LogDebug($"Request index list: {requestId}");

                            IndexListRequestHandler handler = new IndexListRequestHandler(context, this.kustoManager, requestId, this.logger);

                            responseString = handler.PrepareResponse(requestInputString);

                            requestTraceIsOn = true;
                            //requestAnsweredSuccessfully = true;
                        }
                        else if (DetailedIndexListRequestHandler.Mine(request.RawUrl, requestInputString))
                        {
                            this.logger.LogDebug($"Request getting detailed index list and schemas: {requestId}");

                            DetailedIndexListRequestHandler handler = new DetailedIndexListRequestHandler(context, this.kustoManager, requestId, this.logger);

                            responseString = handler.PrepareResponse(requestInputString);

                            requestTraceIsOn = true;
                            //requestAnsweredSuccessfully = true;
                        }
                        else if (IndexDetailsRequestHandler.Mine(request.RawUrl, requestInputString))
                        {
                            this.logger.LogDebug($"Request getting index details and schema: {requestId}");

                            IndexDetailsRequestHandler handler = new IndexDetailsRequestHandler(context, this.kustoManager, requestId, this.logger);

                            responseString = handler.PrepareResponse(requestInputString);

                            requestTraceIsOn = true;
                            //requestAnsweredSuccessfully = (responseString != null);
                        }
                    }
                    else if (request.RawUrl.StartsWith(@"/_msearch"))
                    {
                        // This request should be routed to Kusto
                        try
                        {
                            requestTraceIsOn = true;

                            string body = this.GetRequestBody(request, requestInputStream);

                            // the body is in NDJson. TODO: probably there's a better way to do this...
                            string[] lines = body.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries);

                            // TODO: add ability to handle multiple queries
                            this.logger.LogDebug($"Elastic search request:\n{lines[1]}");
                            string translatedKqlQuery = this.translator.Translate(lines[0], lines[1]);
                            this.logger.LogDebug($"Translated query:\n{translatedKqlQuery}");
                            this.tracer.WriteFile($"{request.RequestTraceIdentifier}.KQL.json", translatedKqlQuery);

                            ElasticResponse kustoResults = this.kustoManager.ExecuteQuery(translatedKqlQuery);
                            responseString = JsonConvert.SerializeObject(kustoResults);

                            requestAnsweredSuccessfully = true;
                        }
                        catch (Exception ex)
                        {
                            this.logger.LogError(ex, "Failed to translate query.");
                        }
                    }

                    var remoteResponse = this.PassThrough(request, this.remoteEndpoint, this.TimeoutInMilliSeconds, requestInputStream);
                    Stream remoteResponseStream = null;

                    if (remoteResponse != null)
                    {
                        // use a stream we can read more than once
                        remoteResponseStream = new MemoryStream();
                        remoteResponse.GetResponseStream().CopyStream(remoteResponseStream);
                    }

                    if (requestAnsweredSuccessfully)
                    {
                        byte[] kustoResultsContent = Encoding.ASCII.GetBytes(responseString);

                        kustoResultsStream = new MemoryStream(kustoResultsContent);

                        if (requestTraceIsOn)
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

                        // Send the respose back
                        var output = response.OutputStream;
                        remoteResponseStream.CopyStream(output);
                        output.Close();
                    }

                    if (requestTraceIsOn)
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

        private HttpWebResponse PassThrough(HttpListenerRequest request, string remoteEndpoint, int timeoutInMilliSeconds, MemoryStream memoryStream)
        {
            try
            {
                string[] bodylessMethods = { "GET", "HEAD" };

                var requestString = $"{remoteEndpoint}{request.RawUrl}";
                var remoteRequest = WebRequest.Create(requestString) as HttpWebRequest;
                remoteRequest.AllowAutoRedirect = false;
                remoteRequest.KeepAlive = request.KeepAlive;
                remoteRequest.Proxy.Credentials = CredentialCache.DefaultCredentials;
                remoteRequest.Method = request.HttpMethod;
                remoteRequest.ContentType = request.ContentType;

                var cookies = new CookieContainer();
                cookies.Add(new Uri(requestString), request.Cookies);
                remoteRequest.CookieContainer = cookies;
                remoteRequest.Timeout = timeoutInMilliSeconds;

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
            catch (Exception ex)
            {
                this.logger.LogError(ex, $"PassThrough request to: {request.RawUrl} ended with an exception");
                return null;
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
