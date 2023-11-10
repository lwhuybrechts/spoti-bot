using SpotifyAPI.Web;
using SpotiBot.Library.BusinessModels.Spotify;

namespace SpotiBot.Library.Spotify.Api
{
    public interface ISpotifyClientFactory
    {
        ISpotifyClient Create(AuthorizationToken token);
    }
}