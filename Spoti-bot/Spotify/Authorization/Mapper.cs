using SpotifyAPI.Web;

namespace Spoti_bot.Spotify.Authorization
{
    public class Mapper : IMapper
    {
        public AuthorizationCodeTokenResponse Map(AuthorizationToken source) => new()
        {
            AccessToken = source.AccessToken,
            TokenType = source.TokenType,
            ExpiresIn = source.ExpiresIn,
            Scope = source.Scope,
            RefreshToken = source.RefreshToken,
            CreatedAt = source.CreatedAt
        };

        public AuthorizationToken Map(AuthorizationCodeTokenResponse source) => new()
        {
            AccessToken = source.AccessToken,
            TokenType = source.TokenType,
            ExpiresIn = source.ExpiresIn,
            Scope = source.Scope,
            RefreshToken = source.RefreshToken,
            CreatedAt = source.CreatedAt
        };
    }
}
