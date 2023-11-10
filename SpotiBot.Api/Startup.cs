using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using SpotiBot.Api;
using SpotiBot.Api.Bot;
using SpotiBot.Api.Bot.HandleUpdate;
using SpotiBot.Api.Bot.HandleUpdate.Commands;
using SpotiBot.Api.Bot.HandleUpdate.Dto;
using SpotiBot.Api.Bot.Votes;
using SpotiBot.Api.Bot.WebApp;
using SpotiBot.Api.Library.Options;
using SpotiBot.Api.Spotify;
using SpotiBot.Api.Spotify.Authorization;
using SpotiBot.Api.Spotify.Tracks.AddTrack;
using SpotiBot.Api.Spotify.Tracks.RemoveTrack;
using SpotiBot.Api.Spotify.Tracks.SyncHistory;
using SpotiBot.Api.Spotify.Tracks.SyncTracks;
using SpotiBot.Data;
using SpotiBot.Data.Services;
using SpotiBot.Library;
using Telegram.Bot;
using VoteService = SpotiBot.Api.Bot.Votes.VoteService;

[assembly: FunctionsStartup(typeof(Startup))]
namespace SpotiBot.Api
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
                .AddData()
                .AddOptions()
                .AddMapper()
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
                .AddTransient<Bot.Votes.IVoteService, VoteService>()
                .AddTransient<IVoteTextHelper, VoteTextHelper>()
                .AddTransient<IWebAppValidationService, WebAppValidationService>()
                .AddTransient<IAuthService, AuthService>()

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
                .AddTransient<IReplyMessageService, ReplyMessageService>();
        }

        /// <summary>
        /// Add a mapper.
        /// </summary>
        /// <param name="services">The services we want to add our mapper to.</param>
        public static IServiceCollection AddMapper(this IServiceCollection services)
        {
            return services
                .AddTransient<Bot.WebApp.IMapper, Mapper>()
                .AddTransient<Bot.Chats.IMapper, Bot.Chats.Mapper>()
                .AddTransient<Bot.Users.IMapper, Bot.Users.Mapper>();
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
                .AddAndBindOptions<TelegramOptions>()
                .AddAndBindOptions<TestOptions>();
        }
    }
}