using SpotiBot.Data.Services;
using SpotiBot.Library.Spotify.Api;
using SpotifyAPI.Web;
using System.Threading.Tasks;

namespace SpotiBot.Api.Spotify
{
    public class AuthService : IAuthService
    {
        private readonly ISpotifyClientFactory _spotifyClientFactory;
        private readonly IAuthorizationTokenService _authorizationTokenService;

        public AuthService(
            ISpotifyClientFactory spotifyClientFactory,
            IAuthorizationTokenService authorizationTokenService)
        {
            _spotifyClientFactory = spotifyClientFactory;
            _authorizationTokenService = authorizationTokenService;
        }

        public async Task<ISpotifyClient> GetClient(long userId)
        {
            var token = await _authorizationTokenService.Get(userId);
            if (token == null)
                return null;

            return _spotifyClientFactory.Create(token);
        }
    }
}
