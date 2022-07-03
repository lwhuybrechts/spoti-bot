using Microsoft.Extensions.Options;
using Spoti_bot.Library.Options;
using Spoti_bot.Spotify.Authorization;
using SpotifyAPI.Web;
using System;
using System.Threading.Tasks;

namespace Spoti_bot.Spotify.Api
{
    public class SpotifyClientFactory : ISpotifyClientFactory
    {
        private readonly IAuthorizationTokenRepository _authorizationTokenRepository;
        private readonly IMapper _mapper;
        private readonly SpotifyOptions _spotifyOptions;

        /// <summary>
        /// The SpotifyClient has a dependency on the userId of the user that sent a message.
        /// So, we create the client at runtime with a factory.
        /// </summary>
        public SpotifyClientFactory(
            IAuthorizationTokenRepository authorizationTokenRepository,
            IMapper mapper,
            IOptions<SpotifyOptions> spotifyOptions)
        {
            _authorizationTokenRepository = authorizationTokenRepository;
            _mapper = mapper;
            _spotifyOptions = spotifyOptions.Value;
        }

        /// <summary>
        /// Create a client to do calls to the spotify api with.
        /// </summary>
        /// <returns>A SpotifyClient, or null of no accessToken could be found.</returns>
        public async Task<ISpotifyClient> Create(long userId)
        {
            var token = await _authorizationTokenRepository.Get(userId);

            if (token == null)
                return null;

            // Map the token to a model the Spotify library can work with.
            var tokenResponse = _mapper.Map(token);

            // TODO: inject singleton httpclient from startup.
            var config = SpotifyClientConfig
                .CreateDefault()
                .WithAuthenticator(new AuthorizationCodeAuthenticator(_spotifyOptions.ClientId, _spotifyOptions.Secret, tokenResponse))
                .WithRetryHandler(new SimpleRetryHandler() { RetryAfter = TimeSpan.FromSeconds(1), TooManyRequestsConsumesARetry = true });

            return new SpotifyClient(config);
        }
    }
}
