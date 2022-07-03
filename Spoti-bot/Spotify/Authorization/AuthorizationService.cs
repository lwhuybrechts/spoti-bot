using Microsoft.Extensions.Options;
using Spoti_bot.Bot;
using Spoti_bot.Library.Exceptions;
using Spoti_bot.Library.Options;
using SpotifyAPI.Web;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Spoti_bot.Spotify.Authorization
{
    public class AuthorizationService : IAuthorizationService
    {
        private readonly IAuthorizationTokenRepository _authorizationTokenRepository;
        private readonly ILoginRequestService _loginRequestService;
        private readonly ISendMessageService _sendMessageService;
        private readonly IMapper _mapper;
        private readonly SpotifyOptions _spotifyOptions;
        private readonly AzureOptions _azureOptions;

        public AuthorizationService(
            IAuthorizationTokenRepository authorizationTokenRepository,
            ILoginRequestService loginRequestService,
            IMapper mapper,
            IOptions<SpotifyOptions> spotifyOptions,
            IOptions<AzureOptions> azureOptions)
        {
            _authorizationTokenRepository = authorizationTokenRepository;
            _loginRequestService = loginRequestService;
            _mapper = mapper;
            _spotifyOptions = spotifyOptions.Value;
            _azureOptions = azureOptions.Value;
        }

        /// <summary>
        /// Get an url to the spotify login web page, where we can authorize our bot.
        /// </summary>
        /// <returns>The url to the spotify login page.</returns>
        public async Task<Uri> CreateLoginRequest(long userId, LoginRequestReason reason, long? groupChatId, long privateChatId, string trackId = null)
        {
            var loginRequest = await _loginRequestService.Create(reason, userId, groupChatId, privateChatId, trackId);

            // Make sure we can add tracks to the queue for all users.
            var scopes = new List<string> { Scopes.UserModifyPlaybackState };

            // For group chats or the LoginLink command also request access to playlists, so we can add tracks.
            if (reason == LoginRequestReason.AddBotToGroupChat ||
                reason == LoginRequestReason.LoginLinkCommand)
                scopes.AddRange(new [] { Scopes.PlaylistModifyPrivate, Scopes.PlaylistModifyPublic });

            return new SpotifyAPI.Web.LoginRequest(GetCallbackUri(), _spotifyOptions.ClientId, SpotifyAPI.Web.LoginRequest.ResponseType.Code)
            {
                Scope = scopes,
                State = loginRequest.Id
            }.ToUri();
        }

        /// <summary>
        /// Request and save an AuthorizationToken, with the code we got from the spotify login callback.
        /// </summary>
        /// <param name="code">The code the spotify login page sends us after a successful login.</param>
        /// <param name="loginRequestId">The id of the loginRequest that was saved in storage.</param>
        public async Task<LoginRequest> RequestAndSaveAuthorizationToken(string code, string loginRequestId)
        {
            var loginRequest = await _loginRequestService.Get(loginRequestId);

            if (loginRequest == null)
                throw new LoginRequestNullException(loginRequestId);

            var tokenRequest = new AuthorizationCodeTokenRequest(_spotifyOptions.ClientId, _spotifyOptions.Secret, code, GetCallbackUri());

            // TODO: reuse httpclient from startup.
            var accessToken = await new OAuthClient().RequestToken(tokenRequest);

            if (accessToken == null)
                throw new AccessTokenNullException();

            var token = _mapper.Map(accessToken);
            token.UserId = loginRequest.UserId;

            // Save the token.
            await _authorizationTokenRepository.Upsert(token);

            // The login request has been handled, delete it.
            await _loginRequestService.Delete(loginRequest);

            return loginRequest;
        }

        private Uri GetCallbackUri()
        {
            var baseUri = new Uri(_azureOptions.FunctionAppUrl);

            return new Uri(baseUri, $"api/{nameof(Callback).ToLower()}");
        }
    }
}
