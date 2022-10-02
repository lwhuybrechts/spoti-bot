using SpotifyAPI.Web;

namespace SpotiBot.Spotify.Authorization
{
    public interface IMapper
    {
        AuthorizationCodeTokenResponse Map(AuthorizationToken source);
        AuthorizationToken Map(AuthorizationCodeTokenResponse source);
    }
}
