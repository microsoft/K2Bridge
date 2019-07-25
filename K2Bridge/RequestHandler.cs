using System;
using System.Linq;
using System.Net;
using System.Data;
using System.Collections.Generic;
using Newtonsoft.Json;
using K2Bridge.KustoConnector;

namespace K2Bridge
{
    internal class RequestHandler
    {
        protected HttpListenerContext context;


        protected KustoManager kusto;

        protected Serilog.ILogger Logger { get; set; }

        public static Serilog.ILogger StaticLogger { get; set; }

        public RequestHandler()
        {
            this.Logger = StaticLogger;
        }
    }
}
