using SpotiBot.Library.BusinessModels.Spotify;
using System.Threading.Tasks;

namespace SpotiBot.Data.Services
{
    public interface IPlaylistService
    {
        Task<Playlist?> Get(string id);
        Task<Playlist> Save(Playlist playlist);
    }
}