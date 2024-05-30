using SpotiBot.Library;
using System;
using System.Runtime.Serialization;

namespace SpotiBot.Spotify.Authorization
{
    public class LoginRequest : MyTableEntity
    {
        [IgnoreDataMember]
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
        [IgnoreDataMember]
        public LoginRequestReason Reason
        {
            get { return (LoginRequestReason)ReasonValue; }
            set { ReasonValue = (int)value; }
        }
        public DateTimeOffset ExpiresAt { get; set; }
    }
}
