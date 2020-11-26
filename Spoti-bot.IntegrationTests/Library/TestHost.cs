using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;

namespace Spoti_bot.IntegrationTests.Library
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
			
			var host = new HostBuilder()
				.ConfigureHostConfiguration(configurationBuilder =>
				{
					configurationBuilder.AddLocalSettings("Spoti-bot", "Spoti-bot", "local.settings.json");
				})
				// Azure Functions are built on top of the Web Jobs SDK.
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
