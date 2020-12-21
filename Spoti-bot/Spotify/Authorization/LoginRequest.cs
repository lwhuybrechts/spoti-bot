using Microsoft.Azure.Cosmos.Table;
using System;

namespace Spoti_bot.Spotify.Authorization
{
    public class LoginRequest : TableEntity
    {
        [IgnoreProperty]
        public string Id
        {
            get { return RowKey; }
            set { RowKey = value; }
        }

        public long ChatId { get; set; }
        public long UserId { get; set; }
        public DateTimeOffset ExpiresAt { get; set; }
    }
}
