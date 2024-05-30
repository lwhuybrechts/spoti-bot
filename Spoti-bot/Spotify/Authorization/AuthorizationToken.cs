using SpotiBot.Library;
using System;
using System.Runtime.Serialization;

namespace SpotiBot.Spotify.Authorization
{
    public class AuthorizationToken : MyTableEntity
    {
        [IgnoreDataMember]
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
    }
}
