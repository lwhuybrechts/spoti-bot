namespace SpotiView.ApiModels
{
    public class SpotifyAccessToken
    {
        public string AccessToken { get; set; }
        public string TokenType { get; set; }
        public int ExpiresIn { get; set; }
        public string Scope { get; set; }
        public string RefreshToken { get; set; }
        public DateTime CreatedAt { get; set; }

        public SpotifyAccessToken(string accessToken, string tokenType, int expiresIn, string scope, string refreshToken, DateTime createdAt)
        {
            AccessToken = accessToken;
            TokenType = tokenType;
            ExpiresIn = expiresIn;
            Scope = scope;
            RefreshToken = refreshToken;
            CreatedAt = createdAt;
        }
    }
}