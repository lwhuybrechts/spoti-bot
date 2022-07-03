using SpotifyAPI.Web;

namespace Spoti_bot.Spotify.Authorization
{
    public interface IMapper
    {
        AuthorizationCodeTokenResponse Map(AuthorizationToken source);
        AuthorizationToken Map(AuthorizationCodeTokenResponse source);
    }
}
