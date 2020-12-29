using AutoMapper;
using Microsoft.Extensions.Options;
using Spoti_bot.Bot;
using Spoti_bot.Bot.HandleUpdate.Commands;
using Spoti_bot.Library;
using Spoti_bot.Library.Exceptions;
using Spoti_bot.Library.Options;
using SpotifyAPI.Web;
using System;
using System.Threading.Tasks;

namespace Spoti_bot.Spotify.Authorization
{
    public class AuthorizationService : IAuthorizationService
    {
        private readonly IAuthorizationTokenRepository _authorizationTokenRepository;
        private readonly ILoginRequestService _loginRequestService;
        private readonly ISendMessageService _sendMessageService;
        private readonly IKeyboardService _keyboardService;
        private readonly IMapper _mapper;
        private readonly SpotifyOptions _spotifyOptions;
        private readonly AzureOptions _azureOptions;

        public AuthorizationService(
            IAuthorizationTokenRepository authorizationTokenRepository,
            ILoginRequestService loginRequestService,
            ISendMessageService sendMessageService,
            IKeyboardService keyboardService,
            IMapper mapper,
            IOptions<SpotifyOptions> spotifyOptions,
            IOptions<AzureOptions> azureOptions)
        {
            _authorizationTokenRepository = authorizationTokenRepository;
            _loginRequestService = loginRequestService;
            _sendMessageService = sendMessageService;
            _keyboardService = keyboardService;
            _mapper = mapper;
            _spotifyOptions = spotifyOptions.Value;
            _azureOptions = azureOptions.Value;
        }

        /// <summary>
        /// Get an url to the spotify login web page, where we can authorize our bot.
        /// </summary>
        /// <returns>The url to the spotify login page.</returns>
        public async Task<Uri> CreateLoginRequest(long userId, long? groupChatId, long privateChatId)
        {
            var loginRequest = await _loginRequestService.Create(userId, groupChatId, privateChatId);

            return new SpotifyAPI.Web.LoginRequest(GetCallbackUri(), _spotifyOptions.ClientId, SpotifyAPI.Web.LoginRequest.ResponseType.Code)
            {
                Scope = new[] { Scopes.PlaylistModifyPrivate, Scopes.PlaylistModifyPublic, Scopes.UserModifyPlaybackState },
                State = loginRequest.Id
            }.ToUri();
        }

        /// <summary>
        /// Request and save an AuthorizationToken, with the code we got from the spotify login callback.
        /// </summary>
        /// <param name="code">The code the spotify login page sends us after a successful login.</param>
        /// <param name="loginRequestId">The id of the loginRequest that was saved in storage.</param>
        public async Task RequestAndSaveAuthorizationToken(string code, string loginRequestId)
        {
            var loginRequest = await _loginRequestService.Get(loginRequestId);

            if (loginRequest == null)
                throw new LoginRequestNullException(loginRequestId);

            var tokenRequest = new AuthorizationCodeTokenRequest(_spotifyOptions.ClientId, _spotifyOptions.Secret, code, GetCallbackUri());

            // TODO: reuse httpclient from startup.
            var accessToken = await new OAuthClient().RequestToken(tokenRequest);

            if (accessToken == null)
                throw new AccessTokenNullException();

            var token = _mapper.Map<AuthorizationToken>(accessToken);
            token.UserId = loginRequest.UserId;

            // Save the token.
            await _authorizationTokenRepository.Upsert(token);

            // The login request has been handled, delete it.
            await _loginRequestService.Delete(loginRequest);

            await RespondInChat(loginRequest);
        }

        private async Task RespondInChat(LoginRequest loginRequest)
        {
            const string successMessage = "Spoti-bot is now authorized, awesome!";

            if (!loginRequest.GroupChatId.HasValue)
            {
                // Answer in the private chat.
                await _sendMessageService.SendTextMessage(loginRequest.PrivateChatId, successMessage);
                return;
            }

            // Answer in the private chat.
            var privateChatText = successMessage + "\n\nPlease return to the group chat for the last step.";
            await _sendMessageService.SendTextMessage(loginRequest.PrivateChatId, privateChatText);

            // Answer in the group chat.
            var groupChatText = successMessage + $"\n\nThe last step is to set the desired playlist with the {Command.SetPlaylist.ToDescriptionString()} command.";
            await _sendMessageService.SendTextMessage(loginRequest.GroupChatId.Value, groupChatText);
        }

        private Uri GetCallbackUri()
        {
            var baseUri = new Uri(_azureOptions.FunctionAppUrl);

            return new Uri(baseUri, $"api/{nameof(Callback).ToLower()}");
        }
    }
}
