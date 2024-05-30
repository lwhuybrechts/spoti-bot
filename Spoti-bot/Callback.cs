using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Options;
using Sentry;
using SpotiBot.Bot;
using SpotiBot.Bot.Chats;
using SpotiBot.Bot.HandleUpdate.Commands;
using SpotiBot.Library.Exceptions;
using SpotiBot.Library.Extensions;
using SpotiBot.Spotify.Api;
using SpotiBot.Spotify.Authorization;
using SpotiBot.Spotify.Tracks;
using System;
using System.Threading.Tasks;

namespace SpotiBot
{
    public class Callback
    {
        public const string SuccessMessage = "Spoti-bot is now authorized, enjoy!";
        public const string ErrorMessage = "Could not authorize Spoti-bot, code is invalid.";

        private readonly IAuthorizationService _spotifyAuthorizationService;
        private readonly ISendMessageService _sendMessageService;
        private readonly IChatRepository _chatRepository;
        private readonly ITrackRepository _trackRepository;
        private readonly ISpotifyClientFactory _spotifyClientFactory;
        private readonly ISpotifyClientService _spotifyClientService;
        private readonly Library.Options.SentryOptions _sentryOptions;

        public Callback(
            IAuthorizationService spotifyAuthorizationService,
            ISendMessageService sendMessageService,
            IChatRepository chatRepository,
            ITrackRepository trackRepository,
            ISpotifyClientFactory spotifyClientFactory,
            ISpotifyClientService spotifyClientService,
            IOptions<Library.Options.SentryOptions> sentryOptions)
        {
            _spotifyAuthorizationService = spotifyAuthorizationService;
            _sendMessageService = sendMessageService;
            _chatRepository = chatRepository;
            _trackRepository = trackRepository;
            _spotifyClientFactory = spotifyClientFactory;
            _spotifyClientService = spotifyClientService;
            _sentryOptions = sentryOptions.Value;
        }

        [Function(nameof(Callback))]
        public async Task<IStatusCodeActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequest httpRequest)
        {
            // Setup exception handling.
            using (new SentryExceptionHandler(_sentryOptions.Dsn))
            {
                try
                {
                    (string code, string loginRequestId) = GetQueryParameters(httpRequest);

                    // Request and save an AuthorizationToken which we can use to do calls to the spotify api.
                    var loginRequest = await _spotifyAuthorizationService.RequestAndSaveAuthorizationToken(code, loginRequestId);

                    await RespondInChat(loginRequest);

                    // Send a reply that is visible in the browser where the user just logged in.
                    return new OkObjectResult(SuccessMessage);
                }
                catch (Exception exception)
                {
                    SentrySdk.CaptureException(exception);

                    // Send a reply that is visible in the browser where the user just logged in.
                    return new OkObjectResult(ErrorMessage);
                }
            }
        }

        private static (string, string) GetQueryParameters(HttpRequest httpRequest)
        {
            // Get the code and state from the callback url.
            var code = GetFromQuery("code", httpRequest);
            var loginRequestId = GetFromQuery("state", httpRequest);

            if (string.IsNullOrEmpty(code))
                throw new QueryParameterNullException(nameof(code));

            if (string.IsNullOrEmpty(loginRequestId))
                throw new QueryParameterNullException(nameof(loginRequestId));

            return (code, loginRequestId);
        }

        private static string GetFromQuery(string key, HttpRequest httpRequest)
        {
            if (httpRequest == null || httpRequest.Query == null)
                return string.Empty;

            return httpRequest.Query.ContainsKey(key)
                ? httpRequest.Query[key].ToString()
                : string.Empty;
        }

        // TODO: refactor.
        private async Task RespondInChat(LoginRequest loginRequest)
        {
            const string successMessage = "Spoti-bot is now authorized, awesome!";

            switch (loginRequest.Reason)
            {
                case LoginRequestReason.AddToQueue:
                    var trackId = loginRequest.TrackId;
                    var chatId = loginRequest.GroupChatId;

                    if (string.IsNullOrEmpty(trackId) || !chatId.HasValue)
                    {
                        await _sendMessageService.SendTextMessage(loginRequest.PrivateChatId, successMessage);
                        return;
                    }

                    var chat = await _chatRepository.Get(chatId.Value);

                    if (chat == null)
                    {
                        await _sendMessageService.SendTextMessage(loginRequest.PrivateChatId, successMessage);
                        return;
                    }

                    var track = await _trackRepository.Get(trackId, chat.PlaylistId);

                    if (track == null)
                    {
                        await _sendMessageService.SendTextMessage(loginRequest.PrivateChatId, successMessage);
                        return;
                    }

                    var spotifyClient = await _spotifyClientFactory.Create(loginRequest.UserId);

                    if (!await _spotifyClientService.AddToQueue(spotifyClient, track))
                    {
                        await _sendMessageService.SendTextMessage(loginRequest.PrivateChatId, successMessage);
                        return;
                    }

                    // Answer in the private chat.
                    await _sendMessageService.SendTextMessage(loginRequest.PrivateChatId, $"{successMessage}\n\n_{track.Name}_ is now added to your queue.");
                    return;
                case LoginRequestReason.AddBotToGroupChat:
                    // Answer in the private chat.
                    var privateChatText = $"{successMessage}\n\nPlease return to the group chat for the last step.";
                    await _sendMessageService.SendTextMessage(loginRequest.PrivateChatId, privateChatText);

                    // Answer in the group chat.
                    var groupChatText = $"{successMessage}\n\nThe last step is to set the desired playlist with the {Command.SetPlaylist.ToDescriptionString()} command.";
                    await _sendMessageService.SendTextMessage(loginRequest.GroupChatId.Value, groupChatText);
                    return;
                case LoginRequestReason.LoginLinkCommand:
                    // Answer in the private chat.
                    await _sendMessageService.SendTextMessage(loginRequest.PrivateChatId, successMessage);
                    return;
            }
        }
    }
}
