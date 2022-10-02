using SpotiBot.Library;
using System.Threading.Tasks;

namespace SpotiBot.Spotify.Tracks.SyncHistory
{
    public interface ISyncHistoryService
    {
        Task<int> SyncTracksFromJson(string jsonString);
    }
}