using SpotiBot.Library.Enums;
using System;

namespace SpotiBot.Library.BusinessModels.Spotify
{
    public class LoginRequest
    {
        public string Id { get; set; } = string.Empty;
        public long? GroupChatId { get; set; }
        public long PrivateChatId { get; set; }
        public long UserId { get; set; }
        public string? TrackId { get; set; }
        public LoginRequestReason Reason { get; set; }
        public DateTimeOffset ExpiresAt { get; set; }

        public LoginRequest(string id, long? groupChatId, long privateChatId, long userId, string? trackId, LoginRequestReason reason, DateTimeOffset expiresAt)
        {
            Id = id;
            GroupChatId = groupChatId;
            PrivateChatId = privateChatId;
            UserId = userId;
            TrackId = trackId;
            Reason = reason;
            ExpiresAt = expiresAt;
        }
    }
}
