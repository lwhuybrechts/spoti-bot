using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SpotiBot.Api.Spotify.Tracks.SyncHistory
{
    public interface IParseHistoryJsonService
    {
        Task<List<TrackAdded>> ParseTracks(string jsonString, DateTimeKind dateTimeKind);
    }
}