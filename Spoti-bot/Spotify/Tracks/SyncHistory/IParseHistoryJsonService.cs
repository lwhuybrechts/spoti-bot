using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Spoti_bot.Spotify.Tracks.SyncHistory
{
    public interface IParseHistoryJsonService
    {
        Task<List<TrackAdded>> ParseTracks(string jsonString, DateTimeKind dateTimeKind);
    }
}