using System;

namespace SpotiBot.Library.BusinessModels.Spotify
{
    public class AuthorizationToken
    {
        public long UserId { get; set; }
        public string AccessToken { get; set; }
        public string TokenType { get; set; }
        public int ExpiresIn { get; set; }
        public string Scope { get; set; }
        public string RefreshToken { get; set; }
        public DateTime CreatedAt { get; set; }

        public AuthorizationToken(long userId, string accessToken, string tokenType, int expiresIn, string scope, string refreshToken, DateTime createdAt)
        {
            UserId = userId;
            AccessToken = accessToken;
            TokenType = tokenType;
            ExpiresIn = expiresIn;
            Scope = scope;
            RefreshToken = refreshToken;
            CreatedAt = createdAt;
        }
    }
}
