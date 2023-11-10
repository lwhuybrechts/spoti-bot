using System;

namespace SpotiBot.Library.ApiModels
{
    public class Upvote
    {
        public string TrackId { get; set; }
        public int UserId { get; set; }
        public DateTimeOffset AddedAt { get; set; }

        public Upvote(string trackId, int userId, DateTimeOffset addedAt)
        {
            TrackId = trackId;
            UserId = userId;
            AddedAt = addedAt;
        }
    }
}
