namespace K2Bridge
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Text;
    using System.Collections;
    using K2Bridge.KustoConnector;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    internal class SimpleListener
    {
        public string[] Prefixes { get; set; }

        public string RemoteEndpoint { get; set; }

        public QueryTranslator Translator { get; set; }

        public Serilog.ILogger Logger { get; set; }

        public int TimeoutInMilliSeconds { get; set; } = 5000;

        private const string TracePath = @"../../../Traces";

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

            KustoManager kusto = new KustoManager();

            RequestHandler.StaticLogger = this.Logger;

            // Setup tracing directory
            if (!Directory.Exists(TracePath))
            {
                Directory.CreateDirectory(TracePath);
            }

            listener.Start();
            this.Logger.Information("Proxy is Listening...");
            this.Logger.Information("Press Ctrl+C to exit.");

            while (true)
            {
                try
                {
                    Guid requestId = Guid.NewGuid();

                    // Note: The GetContext method blocks while waiting for a request.
                    HttpListenerContext context = listener.GetContext();
                    HttpListenerRequest request = context.Request;

                    // use a stream we can read more than once
                    var requestInputStream = new MemoryStream();
                    request.InputStream.CopyTo(requestInputStream);
                    requestInputStream.Position = 0;


                    bool requestTraceIsOn = true;
                    bool requestAnsweredSuccessfully = false;

                    requestTraceIsOn = request.RawUrl.StartsWith(@"/.kibana");

                    if (requestTraceIsOn)
                    {
                        // Write the request befaore anything bad might happen. 

                        this.WriteFile($"{requestId}.Request.json", requestInputStream);
                        this.Logger.Debug($"Request: {request.RawUrl}:{requestId}");
                    }

                    var sr = new StreamReader(requestInputStream);
                    string requestInputString = sr.ReadToEnd();
                    requestInputStream.Position = 0;

                    string responseString = null;

                    if (request.RawUrl.StartsWith(@"/.kibana/"))
                    {
                        if (IndexListRequestHandler.Mine(request.RawUrl, requestInputString))
                        {
                            this.Logger.Debug($"Request index list:{requestId}");

                            IndexListRequestHandler handler = new IndexListRequestHandler(context, kusto, requestId);

                            responseString = handler.PrepareResponse(requestInputString);

                            //requestAnsweredSuccessfully = true;
                        }
                        else if (DetailedIndexListRequestHandler.Mine(request.RawUrl, requestInputString))
                        {
                            this.Logger.Debug($"Request Getting detailed index list and schemas:{requestId}");

                            DetailedIndexListRequestHandler handler = new DetailedIndexListRequestHandler(context, kusto, requestId);

                            responseString = handler.PrepareResponse(requestInputString);

                            //requestAnsweredSuccessfully = true;
                        }
                        else if (IndexDetailsRequestHandler.Mine(request.RawUrl, requestInputString))
                        {
                            this.Logger.Debug($"Request Getting index details and schema:{requestId}");

                            IndexDetailsRequestHandler handler = new IndexDetailsRequestHandler(context, kusto, requestId);

                            responseString = handler.PrepareResponse(requestInputString);

                            //requestAnsweredSuccessfully = (responseString != null);
                        }
                    }
                    else if (request.RawUrl.StartsWith(@"/_msearch"))
                    {
                        try
                        {
                            string body = this.GetRequestBody(request, requestInputStream);

                            // the body is in NDJson. TODO: probably there's a better way to do this...
                            // TODO: handle the "index" part
                            string[] lines = body.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries);

                            // TODO: add ability to handle multiple queries
                            this.Logger.Debug($"Elastic search request:\n{lines[1]}");
                            string translatedKqlQuery = this.Translator.Translate(lines[0], lines[1]);
                            this.Logger.Debug($"Translated query:\n{translatedKqlQuery}");

                            ElasticResponse kustoResults = kusto.ExecuteQuery(translatedKqlQuery);
                            responseString = JsonConvert.SerializeObject(kustoResults);

                            this.WriteFile($"{requestId}.KQL.json", translatedKqlQuery);

                            requestAnsweredSuccessfully = true;
                        }
                        catch (Exception ex)
                        {
                            this.Logger.Error(ex, "Failed to translate query.");
                        }
                        finally
                        {
                            if (requestInputStream.Position > 0)
                            {
                                // rewind the stream for another read
                                requestInputStream.Position = 0;
                            }
                        }
                    }

                    var remoteResponse = this.PassThrough(request, this.RemoteEndpoint, this.TimeoutInMilliSeconds, requestInputStream);

                    // use streams we can read more than once
                    var passthroughResposeStream = new MemoryStream();
                    remoteResponse.GetResponseStream().CopyTo(passthroughResposeStream);

                    Stream responseStream = null;
                    Stream kustoResultsStream = null;
                    Int64 kustoResultsStreamLength = 0;

                    HttpListenerResponse response = context.Response;

                    if (null != responseString)
                    {
                        byte[] kustoResultsContent = Encoding.ASCII.GetBytes(responseString);
                        kustoResultsStream = new MemoryStream(kustoResultsContent);
                        kustoResultsStreamLength = kustoResultsContent.LongLength;
                    }

                    if (requestAnsweredSuccessfully)
                    {
                        responseStream = kustoResultsStream;

                        response.StatusCode = 200;
                        response.ContentLength64 = kustoResultsStreamLength;
                        response.ContentType = "application/json";
                    }
                    else 
                    {
                        // We didn't answer the request yet, so use the elastic pass-through response
                        // Obtain a response object.
                        responseStream = passthroughResposeStream;

                        response.StatusCode = (int)remoteResponse.StatusCode;
                        response.ContentLength64 = remoteResponse.ContentLength;
                        response.ContentType = remoteResponse.ContentType;
                    }

                    responseStream.Position = 0;
                    responseStream.CopyTo(response.OutputStream);
                    response.OutputStream.Close();

                    if (requestTraceIsOn)
                    {
                        this.WriteFile($"{requestId}.ElasticResponse.json", passthroughResposeStream);
                        if (kustoResultsStream!= null)
                        {
                            this.WriteFile($"{requestId}.K2Response.json", kustoResultsStream);
                        }
                    }

                    ResponseComparer rc = new ResponseComparer(this.Logger);

                    rc.CompareStreams(passthroughResposeStream, kustoResultsStream);
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
                        // request.InputStream.CopyTo(stream);

                        // This is a fallback since we already read the source stream
                        memoryStream.CopyTo(stream);
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

        private void WriteFile(string filename, string content)
        {
            try
            {
                using (StreamWriter outputFile = new StreamWriter(Path.Combine(TracePath, filename)))
                {
                    outputFile.Write(content);
                }
            }
            catch (Exception ex)
            {
                this.Logger.Error(ex, "Failed to write trace files.");
                this.Logger.Warning($"Create folder {TracePath} to dump the content of translated queries");
            }
        }

        private void WriteFile(string filename, Stream content)
        {
            try
            {
                using (FileStream outputFile = new FileStream(Path.Combine(TracePath, filename), FileMode.CreateNew))
                {
                    content.Position = 0;
                    content.CopyTo(outputFile);
                    content.Position = 0;

                    outputFile.Flush();
                }
            }
            catch (Exception ex)
            {
                this.Logger.Error(ex, "Failed to write trace files.");
                this.Logger.Warning($"Create folder {TracePath} to dump the content of translated queries");
            }
        }

        private string GetRequestBody(HttpListenerRequest request, MemoryStream bodyMemoryStream)
        {
            if (!request.HasEntityBody)
            {
                return null;
            }

            using (var reader = new StreamReader(bodyMemoryStream, request.ContentEncoding, false, 4096, true))
            {
                return reader.ReadToEnd();
            }
        }
    }
}
