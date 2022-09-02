using System.Threading.Tasks;

namespace Spoti_bot.Spotify.Playlists
{
    public interface IPlaylistRepository
    {
        Task<Playlist> Get(string rowKey, string partitionKey = "");
        Task<Playlist> Get(Playlist item);
        Task<Playlist> Upsert(Playlist item);
        Task Delete(Playlist item);
    }
}