﻿using Spoti_bot.Spotify.Tracks.SyncHistory;
using SpotifyAPI.Web;
using System.Collections.Generic;

namespace Spoti_bot.Spotify.Tracks
{
    public interface IMapper
    {
        Track Map(FullTrack source);
        List<Track> Map(List<FullTrack> source);
        Track Map(string source);
        Track Map(TrackAdded source);
        List<Track> Map(List<TrackAdded> source);
    }
}
