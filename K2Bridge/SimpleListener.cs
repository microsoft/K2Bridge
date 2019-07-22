namespace K2Bridge
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Net;

    internal static class SimpleListener
    {
        public static void Start(string[] prefixes, string remoteEndpoint, QueryTranslator translator, int timeoutInMilliSeconds = 5000)
        {
            if (!HttpListener.IsSupported)
            {
                Console.WriteLine("OS doesn't support using the HttpListener class.");
                return;
            }

            // URI prefixes are required,
            // for example "http://contoso.com:8080/index/".
            if (prefixes == null || prefixes.Length == 0)
            {
                throw new ArgumentException("prefixes");
            }

            // Create a listener.
            HttpListener listener = new HttpListener();

            // Add the prefixes.
            foreach (string s in prefixes)
            {
                listener.Prefixes.Add(s);
            }

            listener.Start();
            Console.WriteLine("Proxy is Listening...");
            Console.WriteLine("Press Ctrl+C to exit.");

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

                    if (request.RawUrl.StartsWith(@"/_msearch"))
                    {
                        // This request should be routed to Kusto
                        Console.WriteLine("Data Search");

                        try
                        {
                            string body = GetRequestBody(request, requestInputStream);

                            // the body is in NDJson. TODO: probably there's a better way to do this...
                            // TODO: handle the "index" part
                            string[] lines = body.Split(
                                new[] { "\r\n", "\r", "\n" },
                                StringSplitOptions.RemoveEmptyEntries);

                            // TODO: add ability to handle multiple queries
                            Console.WriteLine($"Sending to translation: {lines[1]}");
                            string translation = translator.Translate(lines[0], lines[1]);
                            Console.WriteLine($"Translated Query: {translation}");
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("Failed to translate.");
                            //throw;
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

                    var remoteResponse = PassThrough(request, remoteEndpoint, timeoutInMilliSeconds, requestInputStream);

                    // Obtain a response object.
                    HttpListenerResponse response = context.Response;

                    response.StatusCode = (int)remoteResponse.StatusCode;
                    response.ContentLength64 = remoteResponse.ContentLength;
                    response.ContentType = remoteResponse.ContentType;

                    var output = response.OutputStream;
                    remoteResponse.GetResponseStream().CopyTo(output);
                    output.Close();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    throw;
                }
            }
        }

        private static HttpWebResponse PassThrough(HttpListenerRequest request, string remoteEndpoint, int timeoutInMilliSeconds, MemoryStream memoryStream)
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
                Console.WriteLine($"PassThrough request to: {request.RawUrl} ended with an exception: {ex.Message}");
                throw;
                return null;
            }
        }

        private static string GetRequestBody(HttpListenerRequest request, MemoryStream bodyMemoryStream)
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
