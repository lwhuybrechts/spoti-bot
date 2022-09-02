using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.WindowsAzure.Storage;
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
            // TODO: Check if the dependency tree is efficient enough.

            // Bot dependencies.
            return services
                .AddTransient<ICommandsService, CommandsService>()
                .AddTransient<IHandleMessageService, HandleMessageService>()
                .AddTransient<IHandleCallbackQueryService, HandleCallbackQueryService>()
                .AddTransient<IHandleInlineQueryService, HandleInlineQueryService>()
                .AddTransient<IHandleCommandService, HandleCommandService>()
                .AddTransient<IHandleInlineQueryCommandService, HandleInlineQueryCommandService>()
                .AddTransient<IUpdateDtoService, UpdateDtoService>()
                .AddTransient<ISendMessageService, SendMessageService>()
                .AddTransient<IKeyboardService, KeyboardService>()
                .AddTransient<IUserService, UserService>()
                .AddTransient<IVoteService, VoteService>()
                .AddTransient<IVoteTextHelper, VoteTextHelper>()
                .AddTransient<IUserRepository, UserRepository>()
                .AddTransient<IVoteRepository, VoteRepository>()
                .AddTransient<IChatRepository, ChatRepository>()
                .AddTransient<IChatMemberRepository, ChatMemberRepository>()

                // TODO: only use 1 http client, so inject it here.
                .AddSingleton<ITelegramBotClient>((serviceProvider) =>
                {
                    var options = serviceProvider.GetService<IOptions<TelegramOptions>>().Value;

                    return new TelegramBotClient(options.AccessToken);
                })

                // Spotify dependencies.
                .AddTransient<IAuthorizationService, AuthorizationService>()
                .AddTransient<ILoginRequestService, LoginRequestService>()
                .AddTransient<IAddTrackService, AddTrackService>()
                .AddTransient<IRemoveTrackService, RemoveTrackService>()
                .AddTransient<ISyncTracksService, SyncTracksService>()
                .AddTransient<ISyncHistoryService, SyncHistoryService>()
                .AddTransient<IParseHistoryJsonService, ParseHistoryJsonService>()
                .AddTransient<ISpotifyLinkHelper, SpotifyLinkHelper>()
                .AddTransient<IReplyMessageService, ReplyMessageService>()
                .AddTransient<IAuthorizationTokenRepository, AuthorizationTokenRepository>()
                .AddTransient<ILoginRequestRepository, LoginRequestRepository>()
                .AddTransient<ITrackRepository, TrackRepository>()
                .AddTransient<IPlaylistRepository, PlaylistRepository>()

                .AddTransient<ISpotifyClientFactory, SpotifyClientFactory>()
                .AddTransient<ISpotifyClientService, SpotifyClientService>();
        }

        /// <summary>
        /// Add classes that are needed for data storage.
        /// </summary>
        /// <param name="services">The services we want to add our storage to.</param>
        public static IServiceCollection AddStorage(this IServiceCollection services)
        {
            // Add the CloudTableClient as a singleton.
            return services.AddSingleton((serviceProvider) =>
            {
                var options = serviceProvider.GetService<IOptions<AzureOptions>>().Value;

                var storageAccount = CloudStorageAccount.Parse(options.StorageAccountConnectionString);
                var cloudTableClient = storageAccount.CreateCloudTableClient();

                return cloudTableClient;
            });
        }

        /// <summary>
        /// Add a mapper.
        /// </summary>
        /// <param name="services">The services we want to add our mapper to.</param>
        public static IServiceCollection AddMapper(this IServiceCollection services)
        {
            return services
                .AddTransient<ApiModels.IMapper, ApiModels.Mapper>()
                .AddTransient<Bot.Chats.IMapper, Bot.Chats.Mapper>()
                .AddTransient<Bot.Users.IMapper, Bot.Users.Mapper>()
                .AddTransient<Spotify.Authorization.IMapper, Spotify.Authorization.Mapper>()
                .AddTransient<Spotify.Playlists.IMapper, Spotify.Playlists.Mapper>()
                .AddTransient<Spotify.Tracks.IMapper, Spotify.Tracks.Mapper>();
        }

        /// <summary>
        /// Get all options from the configuration and bind them to their respective options models.
        /// The configuration contains the appsettings in Azure, or local.settings.json when running local.
        /// </summary>
        /// <param name="services">The services we want to add our options models to.</param>
        public static IServiceCollection AddOptions(this IServiceCollection services)
        {
            return services
                .AddAndBindOptions<SentryOptions>()
                .AddAndBindOptions<AzureOptions>()
                .AddAndBindOptions<SpotifyOptions>()
                .AddAndBindOptions<TelegramOptions>()
                .AddAndBindOptions<TestOptions>();
        }

        private static IServiceCollection AddAndBindOptions<T>(this IServiceCollection services) where T : class
        {
            var optionsName = typeof(T).Name;

            // Add the values from the configuration to this options' type registration.
            services
                .AddOptions<T>()
                .Configure<IConfiguration>((options, configuration) =>
                    configuration.GetSection(optionsName).Bind(options));

            return services;
        }
    }
}