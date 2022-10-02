using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SpotiBot.IntegrationTests.Library;
using System;

namespace SpotiBot.IntegrationTests
{
    /// <summary>
    /// Builds a host that has all dependencies needed to execute a function in our tests.
    /// </summary>
    public class TestHost
    {
        private readonly IServiceProvider _serviceProvider;

        public TestHost()
        {
            var startup = new TestStartup();

            // Normally for integrationtest we would need an in-memory TestServer that we can communicate with using a HTTP client.
            // However, TestServer is an ASP.Net Core specific type and is currently not supported for Azure Functions.
            // Since Azure Functions are built on top of the Web Jobs SDK, bootrap a generic host via the ConfigureWebJobs method.
            var host = new HostBuilder()
                .ConfigureHostConfiguration(configurationBuilder =>
                {
                    configurationBuilder.AddLocalSettings("Spoti-bot", "Spoti-bot", "local.settings.json");
                })
                .ConfigureWebJobs(startup.Configure)
                .Build();

            _serviceProvider = host.Services;
        }

        public T GetService<T>()
        {
            return _serviceProvider.GetRequiredService<T>();
        }
    }
}
