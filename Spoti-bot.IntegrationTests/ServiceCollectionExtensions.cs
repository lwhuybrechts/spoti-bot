using Microsoft.Extensions.DependencyInjection;

namespace SpotiBot.IntegrationTests
{
    public static class ServiceExtensions
    {
        public static IServiceCollection AddTestServices(this IServiceCollection services)
        {
            return services.AddTransient<GenerateUpdateStreamService>();
        }

        public static IServiceCollection ReplaceServices(this IServiceCollection services)
        {
            // It's possible to replace dependencies for integrationtests here. Example:
            // services.Replace(new ServiceDescriptor(typeof(IService), typeof(Service), ServiceLifetime.Transient));

            return services;
        }
    }
}