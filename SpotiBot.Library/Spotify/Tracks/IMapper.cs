using SpotiBot.Library.BusinessModels.Spotify;
using SpotiBot.Library.Enums;
using SpotifyAPI.Web;
using System;
using System.Collections.Generic;

namespace SpotiBot.Library.Spotify.Tracks
{
    public interface IMapper
    {
        Track Map(FullTrack source, string playlistId, long addedByTelegramUserId, DateTimeOffset createdAt, TrackState state);
        //List<Track> Map(List<FullTrack> source);
        Track Map(string source);
    }
}
