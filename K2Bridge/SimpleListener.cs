namespace K2Bridge
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Text;
    using K2Bridge.KustoConnector;
    using Newtonsoft.Json;

    internal class SimpleListener
    {
        private TraceHelper tracer;

        public string[] Prefixes { get; set; }

        public string RemoteEndpoint { get; set; }

        public QueryTranslator Translator { get; set; }

        public Serilog.ILogger Logger { get; set; }

        public KustoManager KustoManager { get; set; }

        public int TimeoutInMilliSeconds { get; set; } = 5000;

        public void Start()
        {
            if (!HttpListener.IsSupported)
            {
                this.Logger.Error("OS doesn't support using the HttpListener class.");
                return;
            }

            // URI prefixes are required,
            // for example "http://contoso.com:8080/index/".
            if (this.Prefixes == null || this.Prefixes.Length == 0)
            {
                throw new ArgumentException("prefixes");
            }

            // Create a listener.
            HttpListener listener = new HttpListener();

            // Add the prefixes.
            foreach (string s in this.Prefixes)
            {
                listener.Prefixes.Add(s);
            }

            this.tracer = new TraceHelper(this.Logger, @"../../../Traces");

            listener.Start();
            this.Logger.Information("Proxy is Listening...");
            this.Logger.Information("Press Ctrl+C to exit.");

            while (true)
            {
                try
                {
                    // Note: The GetContext method blocks while waiting for a request.
                    HttpListenerContext context = listener.GetContext();
                    HttpListenerRequest request = context.Request;

                    // use a stream we can read more than once
                    var requestInputStream = new MemoryStream();
                    StreamUtils.CopyStream(request.InputStream, requestInputStream);

                    bool requestTraceIsOn = false;
                    bool requestAnsweredSuccessfully = false;

                    // Obtain a response object.
                    HttpListenerResponse response = context.Response;
                    response.Headers.Add("X-K2-CorrelationId", request.RequestTraceIdentifier.ToString());

                    if (request.RawUrl.StartsWith(@"/_msearch"))
                    {
                        // This request should be routed to Kusto
                        requestTraceIsOn = true;
                        this.tracer.WriteFile($"{request.RequestTraceIdentifier}.Request.json", requestInputStream);

                        try
                        {
                            string body = this.GetRequestBody(request, requestInputStream);

                            // the body is in NDJson. TODO: probably there's a better way to do this...
                            string[] lines = body.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries);

                            // TODO: add ability to handle multiple queries
                            this.Logger.Debug($"Elastic search request:\n{lines[1]}");
                            string translatedKqlQuery = this.Translator.Translate(lines[0], lines[1]);
                            this.Logger.Debug($"Translated query:\n{translatedKqlQuery}");
                            this.tracer.WriteFile($"{request.RequestTraceIdentifier}.KQL.json", translatedKqlQuery);

                            ElasticResponse kustoResults = this.KustoManager.ExecuteQuery(translatedKqlQuery);
                            byte[] kustoResultsContent = Encoding.ASCII.GetBytes(JsonConvert.SerializeObject(kustoResults));

                            response.StatusCode = (int)HttpStatusCode.OK;
                            response.ContentLength64 = kustoResultsContent.LongLength;
                            response.ContentType = "application/json";

                            var kustoResultsStream = new MemoryStream(kustoResultsContent);
                            StreamUtils.CopyStream(kustoResultsStream, response.OutputStream);

                            response.OutputStream.Close();

                            if (kustoResultsStream != null)
                            {
                                kustoResultsStream.Position = 0;
                                this.tracer.WriteFile($"{request.RequestTraceIdentifier}.TranslatedResponse.json", kustoResultsStream);
                            }

                            requestAnsweredSuccessfully = true;
                        }
                        catch (Exception ex)
                        {
                            this.Logger.Error(ex, "Failed to translate query.");
                        }
                    }

                    var remoteResponse = this.PassThrough(request, this.RemoteEndpoint, this.TimeoutInMilliSeconds, requestInputStream);

                    // use a stream we can read more than once
                    var remoteResposeStream = new MemoryStream();
                    StreamUtils.CopyStream(remoteResponse.GetResponseStream(), remoteResposeStream);

                    if (!requestAnsweredSuccessfully)
                    {
                        // We didn't answer the request yet, so use the elastic pass-through response
                        response.StatusCode = (int)remoteResponse.StatusCode;
                        response.ContentLength64 = remoteResponse.ContentLength;
                        response.ContentType = remoteResponse.ContentType;
                        response.Headers.Add("X-K2-PassThrough", "true");

                        // Send the respose back
                        var output = response.OutputStream;
                        StreamUtils.CopyStream(remoteResposeStream, output);
                        output.Close();
                    }

                    if (requestTraceIsOn)
                    {
                        this.tracer.WriteFile($"{request.RequestTraceIdentifier}.ElasticResponse.json", remoteResposeStream);
                    }
                }
                catch (Exception ex)
                {
                    this.Logger.Error(ex, "An exception...");
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
                        StreamUtils.CopyStream(memoryStream, stream);
                    }
                }

                var remoteResponse = (HttpWebResponse)remoteRequest.GetResponse();

                return remoteResponse;
            }
            catch (Exception ex)
            {
                this.Logger.Error(ex, $"PassThrough request to: {request.RawUrl} ended with an exception");
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
