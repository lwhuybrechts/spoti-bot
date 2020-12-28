using AutoMapper;
using Microsoft.Azure.Cosmos.Table;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Spoti_bot.Bot;
using Spoti_bot.Bot.Chats;
using Spoti_bot.Bot.HandleUpdate;
using Spoti_bot.Bot.HandleUpdate.Commands;
using Spoti_bot.Bot.HandleUpdate.Dto;
using Spoti_bot.Bot.Users;
using Spoti_bot.Bot.Votes;
using Spoti_bot.Library.Options;
using Spoti_bot.Spotify;
using Spoti_bot.Spotify.Api;
using Spoti_bot.Spotify.Authorization;
using Spoti_bot.Spotify.Playlists;
using Spoti_bot.Spotify.Tracks;
using Spoti_bot.Spotify.Tracks.AddTrack;
using Spoti_bot.Spotify.Tracks.RemoveTrack;
using Spoti_bot.Spotify.Tracks.SyncHistory;
using Spoti_bot.Spotify.Tracks.SyncTracks;
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

    /// <summary>
    /// Extension methods that group all dependencies in a logical way.
    /// </summary>
    public static class ServiceExtensions
    {
        public static IServiceCollection AddServices(this IServiceCollection services)
        {
            // TODO: breakpoint all dependencies and check if they are constructed as expected.

            // Bot dependencies.
            services.AddTransient<ICommandsService, CommandsService>();
            services.AddTransient<IHandleMessageService, HandleMessageService>();
            services.AddTransient<IHandleCallbackQueryService, HandleCallbackQueryService>();
            services.AddTransient<IHandleInlineQueryService, HandleInlineQueryService>();
            services.AddTransient<IHandleCommandService, HandleCommandService>();
            services.AddTransient<IHandleInlineQueryCommandService, HandleInlineQueryCommandService>();
            services.AddTransient<IUpdateDtoService, UpdateDtoService>();
            services.AddTransient<ISendMessageService, SendMessageService>();
            services.AddTransient<IKeyboardService, KeyboardService>();
            services.AddTransient<IUserService, UserService>();
            services.AddTransient<IVoteService, VoteService>();
            services.AddTransient<IVoteTextHelper, VoteTextHelper>();
            services.AddTransient<IUserRepository, UserRepository>();
            services.AddTransient<IVoteRepository, VoteRepository>();
            services.AddTransient<IChatRepository, ChatRepository>();
            services.AddTransient<IChatMemberRepository, ChatMemberRepository>();

            // TODO: only use 1 http client, so inject it here.
            services.AddSingleton<ITelegramBotClient>((serviceProvider) =>
            {
                var options = serviceProvider.GetService<IOptions<TelegramOptions>>().Value;

                return new TelegramBotClient(options.AccessToken);
            });

            // Spotify dependencies.
            services.AddTransient<IAuthorizationService, AuthorizationService>();
            services.AddTransient<ILoginRequestService, LoginRequestService>();
            services.AddTransient<IAddTrackService, AddTrackService>();
            services.AddTransient<IRemoveTrackService, RemoveTrackService>();
            services.AddTransient<ISyncTracksService, SyncTracksService>();
            services.AddTransient<ISyncHistoryService, SyncHistoryService>();
            services.AddTransient<IParseHistoryJsonService, ParseHistoryJsonService>();
            services.AddTransient<ISpotifyLinkHelper, SpotifyLinkHelper>();
            services.AddTransient<ISuccessResponseService, SuccessResponseService>();
            services.AddTransient<IAuthorizationTokenRepository, AuthorizationTokenRepository>();
            services.AddTransient<ILoginRequestRepository, LoginRequestRepository>();
            services.AddTransient<ITrackRepository, TrackRepository>();
            services.AddTransient<IPlaylistRepository, PlaylistRepository>();

            services.AddTransient<ISpotifyClientFactory, SpotifyClientFactory>();
            services.AddTransient<ISpotifyClientService, SpotifyClientService>();

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

            services
                .AddOptions<TestOptions>()
                .Configure<IConfiguration>((settings, configuration) =>
                {
                    configuration.GetSection(nameof(TestOptions)).Bind(settings);
                });

            return services;
        }
    }
}