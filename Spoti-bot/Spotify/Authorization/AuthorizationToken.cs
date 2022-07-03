using Microsoft.WindowsAzure.Storage.Table;
using System;

namespace Spoti_bot.Spotify.Authorization
{
    public class AuthorizationToken : TableEntity
    {
        [IgnoreProperty]
        public long UserId
        {
            get { return long.Parse(RowKey); }
            set { RowKey = value.ToString(); }
        }

        public string AccessToken { get; set; }
        public string TokenType { get; set; }
        public int ExpiresIn { get; set; }
        public string Scope { get; set; }
        public string RefreshToken { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsExpired { get; }
    }
}
