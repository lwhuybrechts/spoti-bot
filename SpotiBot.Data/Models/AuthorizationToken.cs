using Microsoft.WindowsAzure.Storage.Table;
using System;

namespace SpotiBot.Data.Models
{
    public class AuthorizationToken : TableEntity
    {
        [IgnoreProperty]
        public long UserId
        {
            get { return long.Parse(RowKey); }
            set { RowKey = value.ToString(); }
        }

        public string AccessToken { get; set; } = string.Empty;
        public string TokenType { get; set; } = string.Empty;
        public int ExpiresIn { get; set; }
        public string Scope { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}
