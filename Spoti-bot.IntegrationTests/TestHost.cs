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
            // Normally for integrationtest we would need an in-memory TestServer that we can communicate with using a HTTP client.
            // However, TestServer is an ASP.Net Core specific type and was not supported for Azure Functions when implementing this.
            var host = new HostBuilder()
                .ConfigureFunctionsWebApplication()
                .ConfigureHostConfiguration(configurationBuilder => configurationBuilder
                    .AddLocalSettings("SpotiBot", "Spoti-bot", "local.settings.json")
                )
                .ConfigureServices(services => services
                    .AddOptions()
                    .AddMapper()
                    .AddStorage()
                    .AddServices()
                    .AddTestServices()
                    .ReplaceServices()
                )
                .Build();

            _serviceProvider = host.Services;
        }

        public T GetService<T>()
        {
            return _serviceProvider.GetRequiredService<T>();
        }
    }
}
