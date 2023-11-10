using Microsoft.WindowsAzure.Storage.Table;
using System;

namespace SpotiBot.Data.Models
{
    public class LoginRequest : TableEntity
    {
        [IgnoreProperty]
        public string Id
        {
            get { return RowKey; }
            set { RowKey = value; }
        }

        public long? GroupChatId { get; set; }
        public long PrivateChatId { get; set; }
        public long UserId { get; set; }
        public string? TrackId { get; set; }
        public int Reason { get; set; }
        public DateTimeOffset ExpiresAt { get; set; }
    }
}
