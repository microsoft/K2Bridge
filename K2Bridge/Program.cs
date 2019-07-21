namespace K2Bridge
{
    using System;
    using System.IO;
    using Newtonsoft.Json;

    public class Program
    {
        public static void Main(string[] args)
        {
            var translator = new QueryTranslator();

            SimpleListener.Start(new string[] { @"http://localhost:8080/" }, @"http://localhost:9200", translator);
        }
    }
}
