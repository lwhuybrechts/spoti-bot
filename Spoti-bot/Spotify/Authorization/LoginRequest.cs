using Microsoft.WindowsAzure.Storage.Table;
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

        public long? GroupChatId { get; set; }
        public long PrivateChatId { get; set; }
        public long UserId { get; set; }
        public string TrackId { get; set; }

        /// <summary>
        /// Used int since Azure Table Storage doesn't support enums.
        /// </summary>
        public int ReasonValue { get; set; }
        [IgnoreProperty]
        public LoginRequestReason Reason
        {
            get { return (LoginRequestReason)ReasonValue; }
            set { ReasonValue = (int)value; }
        }
        public DateTimeOffset ExpiresAt { get; set; }
    }
}
