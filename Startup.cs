using AutoMapper;
using Microsoft.Azure.Cosmos.Table;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Spoti_bot.Bot;
using Spoti_bot.Bot.Commands;
using Spoti_bot.Library.Options;
using Spoti_bot.Spotify;
using Spoti_bot.Spotify.Data;
using Telegram.Bot;

[assembly: FunctionsStartup(typeof(Spoti_bot.Startup))]
namespace Spoti_bot
{
    public class Startup : FunctionsStartup
    {
        /// <summary>
        /// This function is called when the app starts.
        /// </summary>
        /// <param name="builder">The builder we can use to register dependencies of the app.</param>
        public override void Configure(IFunctionsHostBuilder builder)
        {
            // Register all dependencies of the app.
            builder.Services
                .AddOptions()
                .AddMapper()
                .AddStorageConnector()
                .AddServices();
        }
    }

    public static class ServiceExtensions
    {
        public static IServiceCollection AddServices(this IServiceCollection services)
        {
            // TODO: breakpoint all dependencies and check if they are constructed as expected.

            // Bot dependencies.
            services.AddTransient<ICommandsService, CommandsService>();
            services.AddTransient<IHandleMessageService, HandleMessageService>();
            services.AddTransient<IHandleCallbackQueryService, HandleCallbackQueryService>();
            services.AddTransient<ISendMessageService, SendMessageService>();
            services.AddTransient<IUpvoteHelper, UpvoteHelper>();

            // TODO: only use 1 http client, so inject it here.
            services.AddSingleton<ITelegramBotClient>((serviceProvider) =>
            {
                var options = serviceProvider.GetService<IOptions<TelegramOptions>>().Value;

                return new TelegramBotClient(options.AccessToken);
            });

            // Spotify dependencies.
            services.AddTransient<ISpotifyAuthorizationService, SpotifyAuthorizationService>();
            services.AddTransient<ISpotifyAddTrackService, SpotifyAddTrackService>();
            services.AddTransient<ISpotifyLinkHelper, SpotifyLinkHelper>();
            services.AddTransient<IAuthorizationTokenRepository, AuthorizationTokenRepository>();
            services.AddTransient<ITrackRepository, TrackRepository>();

            return services;
        }

        /// <summary>
        /// Add a storage connector, which we can use to interact with objects in our storage.
        /// </summary>
        /// <param name="services">The service we want to add our storage connector to.</param>
        public static IServiceCollection AddStorageConnector(this IServiceCollection services)
        {
            services.AddSingleton((serviceProvider) =>
            {
                var options = serviceProvider.GetService<IOptions<AzureOptions>>().Value;

                var storageAccount = CloudStorageAccount.Parse(options.StorageAccountConnectionString);
                return storageAccount.CreateCloudTableClient();
            });

            return services;
        }

        /// <summary>
        /// Add a mapper to our services.
        /// </summary>
        /// <param name="services">The service we want to add our mapper to.</param>
        public static IServiceCollection AddMapper(this IServiceCollection services)
        {
            // AutoMapper scans all classes in our project that inherit from Profile.
            services.AddAutoMapper(typeof(Startup));
            
            return services;
        }

        /// <summary>
        /// Get all options from the configuration and bind them to their respective options models.
        /// </summary>
        /// <param name="services">The service we want to add our options models to.</param>
        public static IServiceCollection AddOptions(this IServiceCollection services)
        {
            services
                .AddOptions<SentryOptions>()
                .Configure<IConfiguration>((settings, configuration) =>
                {
                    configuration.GetSection(nameof(SentryOptions)).Bind(settings);
                });

            services
                .AddOptions<AzureOptions>()
                .Configure<IConfiguration>((settings, configuration) =>
                {
                    configuration.GetSection(nameof(AzureOptions)).Bind(settings);
                });

            services
                .AddOptions<PlaylistOptions>()
                .Configure<IConfiguration>((settings, configuration) =>
                {
                    configuration.GetSection(nameof(PlaylistOptions)).Bind(settings);
                });

            services
                .AddOptions<SpotifyOptions>()
                .Configure<IConfiguration>((settings, configuration) =>
                {
                    configuration.GetSection(nameof(SpotifyOptions)).Bind(settings);
                });

            services
                .AddOptions<TelegramOptions>()
                .Configure<IConfiguration>((settings, configuration) =>
                {
                    configuration.GetSection(nameof(TelegramOptions)).Bind(settings);
                });

            return services;
        }
    }
}