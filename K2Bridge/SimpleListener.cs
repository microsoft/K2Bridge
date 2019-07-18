namespace K2Bridge
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Text;

    internal static class SimpleListener
    {
        public static void Start(string[] prefixes, string remoteEndpoint, int timeoutInMilliSeconds = 2000)
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

                    if (request.RawUrl.StartsWith(@"/_msearch"))
                    {
                        // This request should be routed to Kusto
                        Console.WriteLine("Data Search");
                    }

                    var remoteResponse = PassThrough(request, remoteEndpoint, timeoutInMilliSeconds);

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
                }
            }
        }

        private static HttpWebResponse PassThrough(HttpListenerRequest request, string remoteEndpoint, int timeoutInMilliSeconds)
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
                    request.InputStream.CopyTo(stream);
                }
            }

            var remoteResponse = (HttpWebResponse)remoteRequest.GetResponse();

            return remoteResponse;
        }
    }

}
