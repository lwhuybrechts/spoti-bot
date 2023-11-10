using System;
using SpotiBot.Library.Enums;

namespace SpotiBot.Library.BusinessModels.Spotify
{
    public class Track
    {
        public string Id { get; set; }
        public string PlaylistId { get; set; }
        public string Name { get; set; }
        public string FirstArtistName { get; set; }
        public string AlbumName { get; set; }
        public long AddedByTelegramUserId { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public TrackState State { get; set; }

        public Track(string id, string playlistId, string name, string firstArtistName, string albumName, long addedByTelegramUserId, DateTimeOffset createdAt, TrackState state)
        {
            Id = id;
            PlaylistId = playlistId;
            Name = name;
            FirstArtistName = firstArtistName;
            AlbumName = albumName;
            AddedByTelegramUserId = addedByTelegramUserId;
            CreatedAt = createdAt;
            State = state;
        }
    }
}