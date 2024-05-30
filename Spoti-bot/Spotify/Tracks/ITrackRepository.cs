using System.Collections.Generic;
using System.Threading.Tasks;

namespace SpotiBot.Spotify.Tracks
{
    public interface ITrackRepository
    {
        Task<Track> Get(string rowKey, string partitionKey = "");
        Task<Track> Get(Track item);
        Task<List<Track>> GetAll();
        Task<List<Track>> GetAllByPartitionKey(string partitionKey);
        Task Upsert(Track item);
        Task Upsert(List<Track> items);
        Task Delete(Track item);
        Task Delete(List<Track> items);
    }
}