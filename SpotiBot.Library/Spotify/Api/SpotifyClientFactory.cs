using Microsoft.Extensions.Options;
using SpotiBot.Library.BusinessModels.Spotify;
using SpotiBot.Library.Options;
using SpotiBot.Library.Spotify.Authorization;
using SpotifyAPI.Web;
using System;

namespace SpotiBot.Library.Spotify.Api
{
    public class SpotifyClientFactory : ISpotifyClientFactory
    {
        private readonly IMapper _mapper;
        private readonly SpotifyOptions _spotifyOptions;

        /// <summary>
        /// The SpotifyClient has a dependency on the userId of the user that sent a message.
        /// So, we create the client at runtime with a factory.
        /// </summary>
        public SpotifyClientFactory(
            IMapper mapper,
            IOptions<SpotifyOptions> spotifyOptions)
        {
            _mapper = mapper;
            _spotifyOptions = spotifyOptions.Value;
        }

        /// <summary>
        /// Create a client to do calls to the spotify api with.
        /// </summary>
        /// <returns>A SpotifyClient, or null of no accessToken could be found.</returns>
        public ISpotifyClient Create(AuthorizationToken token)
        {
            if (token == null)
                throw new ArgumentNullException(nameof(token));

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
