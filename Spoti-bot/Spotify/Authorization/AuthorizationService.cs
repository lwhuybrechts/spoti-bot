using AutoMapper;
using Microsoft.Extensions.Options;
using Spoti_bot.Library.Exceptions;
using Spoti_bot.Library.Options;
using SpotifyAPI.Web;
using System;
using System.Threading.Tasks;

namespace Spoti_bot.Spotify.Authorization
{
    public class AuthorizationService : IAuthorizationService
    {
        private const string _adminTokenRowKey = "admintoken";

        private readonly IAuthorizationTokenRepository _authorizationTokenRepository;
        private readonly IMapper _mapper;
        private readonly SpotifyOptions _spotifyOptions;
        private readonly AzureOptions _azureOptions;

        public AuthorizationService(
            IAuthorizationTokenRepository authorizationTokenRepository,
            IMapper mapper,
            IOptions<SpotifyOptions> spotifyOptions,
            IOptions<AzureOptions> azureOptions)
        {
            _authorizationTokenRepository = authorizationTokenRepository;
            _mapper = mapper;
            _spotifyOptions = spotifyOptions.Value;
            _azureOptions = azureOptions.Value;
        }

        /// <summary>
        /// Create a client to do calls to the spotify api with.
        /// </summary>
        /// <returns>A SpotifyClient, or null of no accessToken could be found.</returns>
        public async Task<ISpotifyClient> CreateSpotifyClient()
        {
            var token = await _authorizationTokenRepository.Get(_adminTokenRowKey);

            // Map the token to a model the Spotify library can work with.
            var tokenResponse = _mapper.Map<AuthorizationCodeTokenResponse>(token);

            // TODO: inject singleton httpclient from startup.
            var config = SpotifyClientConfig
                .CreateDefault()
                .WithAuthenticator(new AuthorizationCodeAuthenticator(_spotifyOptions.ClientId, _spotifyOptions.Secret, tokenResponse))
                .WithRetryHandler(new SimpleRetryHandler() { RetryAfter = TimeSpan.FromSeconds(1), TooManyRequestsConsumesARetry = true });

            return new SpotifyClient(config);
        }

        /// <summary>
        /// Get an url to the spotify login web page, where we can authorize our bot.
        /// </summary>
        /// <returns>The url to the spotify login page.</returns>
        public Uri GetLoginUri()
        {
            return new LoginRequest(GetCallbackUri(), _spotifyOptions.ClientId, LoginRequest.ResponseType.Code)
            {
                Scope = new[] { Scopes.PlaylistModifyPublic, Scopes.UserModifyPlaybackState }
            }.ToUri();
        }

        /// <summary>
        /// Request and save an AuthorizationToken, with the code we got from the spotify login callback.
        /// </summary>
        /// <param name="code">The code the spotify login page sends us after a successful login.</param>
        public async Task RequestAndSaveAuthorizationToken(string code)
        {
            // TODO: reuse httpclient from startup.
            var accessToken = await new OAuthClient().RequestToken(
                new AuthorizationCodeTokenRequest(_spotifyOptions.ClientId, _spotifyOptions.Secret, code, GetCallbackUri())
            );

            if (accessToken == null)
                throw new AccessTokenNullException();

            var token = _mapper.Map<AuthorizationToken>(accessToken);
            token.RowKey = _adminTokenRowKey;

            // Save it.
            await _authorizationTokenRepository.Upsert(token);
        }

        // TODO: move to a helper file.
        private Uri GetCallbackUri()
        {
            var baseUri = new Uri(_azureOptions.FunctionAppUrl);

            return new Uri(baseUri, $"api/{nameof(Callback).ToLower()}");
        }
    }
}
