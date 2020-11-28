using Microsoft.Azure.Cosmos.Table;
using System;

namespace Spoti_bot.Spotify.Authorization
{
    public class AuthorizationToken : TableEntity
    {
        public string AccessToken { get; set; }
        public string TokenType { get; set; }
        public int ExpiresIn { get; set; }
        public string Scope { get; set; }
        public string RefreshToken { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsExpired { get; }
    }
}
