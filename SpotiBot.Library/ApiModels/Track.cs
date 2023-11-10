using System;

namespace SpotiBot.Library.ApiModels
{
    public class Track
    {
        public string Id { get; set; }
        public string PlaylistId { get; set; }
        public long AddedByTelegramUserId { get; set; }
        public DateTimeOffset AddedAt { get; set; }

        public Track(string id, string playlistId, long addedByTelegramUserId, DateTimeOffset addedAt)
        {
            Id = id;
            PlaylistId = playlistId;
            AddedByTelegramUserId = addedByTelegramUserId;
            AddedAt = addedAt;
        }
    }
}
