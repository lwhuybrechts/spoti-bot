using System;

namespace SpotiBot.Api.Spotify.Tracks.SyncHistory
{
    public class TrackAdded
    {
        public long FromId { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public string TrackId { get; set; }

        public TrackAdded(long fromId, DateTimeOffset createdAt, string trackId)
        {
            FromId = fromId;
            CreatedAt = createdAt;
            TrackId = trackId;
        }
    }
}
