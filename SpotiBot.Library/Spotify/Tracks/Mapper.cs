using SpotiBot.Library.BusinessModels.Spotify;
using SpotiBot.Library.Enums;
using SpotifyAPI.Web;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SpotiBot.Library.Spotify.Tracks
{
    public class Mapper : IMapper
    {
        public Track Map(FullTrack source, string playlistId, long addedByTelegramUserId, DateTimeOffset createdAt, TrackState state) => new(
            source.Id,
            playlistId,
            source.Name,
            source.Artists.First().Name,
            source.Album.Name,
            addedByTelegramUserId,
            createdAt,
            state
        );

        //public List<Track> Map(List<FullTrack> source)
        //{
        //    return source.Select(Map).ToList();
        //}

        public Track Map(string source) =>
            throw new NotImplementedException();
        //new ()
        //{
        //    Id = source
        //};
    }
}
