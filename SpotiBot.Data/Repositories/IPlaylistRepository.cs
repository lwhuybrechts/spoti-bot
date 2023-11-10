using SpotiBot.Data.Models;
using System.Threading.Tasks;

namespace SpotiBot.Data.Repositories
{
    public interface IPlaylistRepository
    {
        Task<Playlist> Get(string rowKey, string partitionKey = "");
        Task<Playlist> Get(Playlist item);
        Task<Playlist> Upsert(Playlist playlist);
        Task Delete(Playlist item);
    }
}