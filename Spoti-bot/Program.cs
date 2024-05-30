using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace SpotiBot
{
    public static class Program
    {
        public static void Main()
        {
            new HostBuilder()
                .ConfigureFunctionsWebApplication()
                .ConfigureServices(services => services
                    .AddApplicationInsightsTelemetryWorkerService()
                    .ConfigureFunctionsApplicationInsights()
                    .AddOptions()
                    .AddMapper()
                    .AddStorage()
                    .AddServices()
                )
                .Build()
                .Run();
        }
    }
}
