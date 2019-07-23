namespace K2Bridge
{
    using Serilog;

    internal class Logger
    {
        internal static ILogger GetLogger()
        {
            var log = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.Console()
                .CreateLogger();

            return log;
        }
    }
}
