namespace K2Bridge
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Net;
    using K2Bridge.KustoConnector;

    internal class SimpleListener
    {
        public string[] Prefixes { get; set; }

        public string RemoteEndpoint { get; set; }

        public QueryTranslator Translator { get; set; }

        public Serilog.ILogger Logger { get; set; }

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

            listener.Start();
            this.Logger.Information("Proxy is Listening...");
            this.Logger.Information("Press Ctrl+C to exit.");

            KustoManager kusto = new KustoManager();

            while (true)
            {
                try
                {
                    // Note: The GetContext method blocks while waiting for a request.
                    HttpListenerContext context = listener.GetContext();
                    HttpListenerRequest request = context.Request;

                    // use a stream we can read more than once
                    var requestInputStream = new MemoryStream();
                    request.InputStream.CopyTo(requestInputStream);
                    requestInputStream.Position = 0;

                    // Log
                    string translatedKqlQuery = null;

                    if (request.RawUrl.StartsWith(@"/_msearch"))
                    {
                        // This request should be routed to Kusto
                        try
                        {
                            string body = this.GetRequestBody(request, requestInputStream);

                            // the body is in NDJson. TODO: probably there's a better way to do this...
                            // TODO: handle the "index" part
                            string[] lines = body.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries);

                            // TODO: add ability to handle multiple queries
                            this.Logger.Debug($"Elastic search request:\n{lines[1]}");
                            translatedKqlQuery = this.Translator.Translate(lines[0], lines[1]);
                            this.Logger.Debug($"Translated Query:\n{translatedKqlQuery}");

                            kusto.ExecuteQuery(translatedKqlQuery);
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

                    // use a stream we can read more than once
                    var remoteResposeStream = new MemoryStream();
                    remoteResponse.GetResponseStream().CopyTo(remoteResposeStream);

                    // Obtain a response object.
                    HttpListenerResponse response = context.Response;

                    response.StatusCode = (int)remoteResponse.StatusCode;
                    response.ContentLength64 = remoteResponse.ContentLength;
                    response.ContentType = remoteResponse.ContentType;

                    // Record the inputs and outputs
                    if (!string.IsNullOrEmpty(translatedKqlQuery))
                    {
                        try
                        {
                            Guid g = Guid.NewGuid();

                            WriteFile($"{g}.KQL.json", translatedKqlQuery);
                            WriteFile($"{g}.TranslatedResponse.json", "TBD");
                            WriteFile($"{g}.Request.json", requestInputStream);
                            WriteFile($"{g}.OriginalResponse.json", remoteResposeStream);
                        }
                        catch (Exception ex)
                        {
                            this.Logger.Error(ex, "Failed to write trace files.");
                            this.Logger.Warning($"Create folder {CaseLogPath} to dump the content of translated queries");
                        }
                    }

                    // Send the respose back
                    var output = response.OutputStream;
                    remoteResposeStream.Position = 0;
                    remoteResposeStream.CopyTo(output);
                    output.Close();
                }
                catch (Exception ex)
                {
                    this.Logger.Error(ex.Message);
                    //throw;
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

        private const string CaseLogPath = "..\\..\\..\\..\\Tests\\dump";

        private static void WriteFile(string filename, string content)
        {
            using (StreamWriter outputFile = new StreamWriter(Path.Combine(CaseLogPath, filename)))
            {
                outputFile.Write(content);
            }
        }

        private static void WriteFile(string filename, Stream content)
        {
            using (FileStream outputFile = new FileStream(Path.Combine(CaseLogPath, filename), FileMode.CreateNew))
            {
                content.Position = 0;
                content.CopyTo(outputFile);
                content.Position = 0;

                outputFile.Flush();
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
