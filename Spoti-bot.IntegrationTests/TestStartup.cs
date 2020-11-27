using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Spoti_bot.IntegrationTests.Library;

namespace Spoti_bot.IntegrationTests
{
    public class TestStartup : Startup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            // Setup dependency injection as we normally would.
            base.Configure(builder);

            // Optionally replace some dependencies.
            builder.Services
                .AddTestServices()
                .ReplaceServices();
        }
    }

    public static class ServiceExtensions
    {
        public static IServiceCollection AddTestServices(this IServiceCollection services)
        {
            services.AddTransient<TestOptions>();
            services.AddTransient<GenerateUpdateStreamService>();

            return services;
        }

        public static IServiceCollection ReplaceServices(this IServiceCollection services)
        {
            // It's possible to replace dependencies for integrationtests here.
            // Example:
            // services.Replace(new ServiceDescriptor(typeof(IService), typeof(Service), ServiceLifetime.Transient));

            return services;
        }
    }
}