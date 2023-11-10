using SpotiBot.Library.BusinessModels.Spotify;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SpotiBot.Data.Services
{
    public interface ITrackService
    {
        Task<Track?> Get(string trackId, string playlistId);
        Task<List<Track>> GetByPlaylist(string playlistId);
        Task<Track> Upsert(Track track);
        Task Upsert(List<Track> tracks);
        Task Delete(List<Track> tracks);
    }
}