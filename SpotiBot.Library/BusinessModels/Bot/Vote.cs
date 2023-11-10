using SpotiBot.Library.Enums;
using System;

namespace SpotiBot.Library.BusinessModels.Bot
{
    public class Vote
    {
        public string PlaylistId { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public VoteType Type { get; set; }
        public string TrackId { get; set; }
        public long UserId { get; set; }

        public Vote(string playlistId, DateTimeOffset createdAt, VoteType type, string trackId, long userId)
        {
            PlaylistId = playlistId;
            CreatedAt = createdAt;
            Type = type;
            TrackId = trackId;
            UserId = userId;
        }
    }
}
