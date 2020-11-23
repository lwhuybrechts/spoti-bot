using AutoMapper;
using Microsoft.Azure.Cosmos.Table;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Spoti_bot.Bot;
using Spoti_bot.Bot.Commands;
using Spoti_bot.Bot.Data.Upvotes;
using Spoti_bot.Bot.Data.Users;
using Spoti_bot.Bot.Interfaces;
using Spoti_bot.Library.Options;
using Spoti_bot.Spotify;
using Spoti_bot.Spotify.Data.AuthorizationTokens;
using Spoti_bot.Spotify.Data.Tracks;
using Spoti_bot.Spotify.Interfaces;
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
                .AddStorage()
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
            services.AddTransient<IUserService, UserService>();
            services.AddTransient<IUpvoteService, UpvoteService>();
            services.AddTransient<IUpvoteTextHelper, UpvoteTextHelper>();
            services.AddTransient<IUserRepository, UserRepository>();
            services.AddTransient<IUpvoteRepository, UpvoteRepository>();

            // TODO: only use 1 http client, so inject it here.
            services.AddSingleton<ITelegramBotClient>((serviceProvider) =>
            {
                var options = serviceProvider.GetService<IOptions<TelegramOptions>>().Value;

                return new TelegramBotClient(options.AccessToken);
            });

            // Spotify dependencies.
            services.AddTransient<ISpotifyAuthorizationService, SpotifyAuthorizationService>();
            services.AddTransient<IAddTrackService, AddTrackService>();
            services.AddTransient<ISyncTracksService, SyncTracksService>();
            services.AddTransient<ISpotifyLinkHelper, SpotifyLinkHelper>();
            services.AddTransient<ISuccessResponseService, SuccessResponseService>();
            services.AddTransient<IAuthorizationTokenRepository, AuthorizationTokenRepository>();
            services.AddTransient<ITrackRepository, TrackRepository>();

            // TODO: only use 1 http client, so inject it here.
            services.AddSingleton<ISpotifyClientService, SpotifyClientService>();

            return services;
        }

        /// <summary>
        /// Add classes that are needed for data storage.
        /// </summary>
        /// <param name="services">The service we want to add our storage to.</param>
        public static IServiceCollection AddStorage(this IServiceCollection services)
        {
            // Add the CloudTableClient as a singleton.
            services.AddSingleton((serviceProvider) =>
            {
                var options = serviceProvider.GetService<IOptions<AzureOptions>>().Value;

                var storageAccount = CloudStorageAccount.Parse(options.StorageAccountConnectionString);
                var cloudTableClient = storageAccount.CreateCloudTableClient();

                return cloudTableClient;
            });

            return services;
        }

        /// <summary>
        /// Add a mapper.
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
        /// The configuration contains the appsettings in Azure, or local.settings.json when running local.
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