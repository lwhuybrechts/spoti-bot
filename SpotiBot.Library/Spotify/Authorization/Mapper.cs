using SpotifyAPI.Web;
using SpotiBot.Library.BusinessModels.Spotify;

namespace SpotiBot.Library.Spotify.Authorization
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

        public AuthorizationToken Map(AuthorizationCodeTokenResponse source, long userId) => new(
            userId,
            source.AccessToken,
            source.TokenType,
            source.ExpiresIn,
            source.Scope,
            source.RefreshToken,
            source.CreatedAt
        );
    }
}
