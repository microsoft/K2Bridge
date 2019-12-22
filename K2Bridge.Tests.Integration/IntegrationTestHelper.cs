using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Serilog;

namespace K2Bridge.Tests.Integration
{
    internal static class IntegrationTestHelper
    {
        internal static TestServer GetIntegrationTestServer(string environment)
        {
            return new TestServer(new WebHostBuilder()
                .UseEnvironment(environment)
                .ConfigureAppConfiguration(InitConfiguration)
                .UseStartup<Startup>()
                .UseSerilog());
        }

        private static void InitConfiguration(IConfigurationBuilder configBuilder)
        {
            configBuilder
                .AddJsonFile("appsettings.test.json", false, true)
                .AddJsonFile("appsettings.test.local.json", true, true)
                .Build();
        }
    }
}
