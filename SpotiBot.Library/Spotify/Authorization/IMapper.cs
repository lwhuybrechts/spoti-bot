using SpotifyAPI.Web;
using SpotiBot.Library.BusinessModels.Spotify;

namespace SpotiBot.Library.Spotify.Authorization
{
    public interface IMapper
    {
        AuthorizationCodeTokenResponse Map(AuthorizationToken source);
        AuthorizationToken Map(AuthorizationCodeTokenResponse source, long userId);
    }
}
