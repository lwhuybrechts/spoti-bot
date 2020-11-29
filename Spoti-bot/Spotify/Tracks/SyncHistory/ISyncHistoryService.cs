using Spoti_bot.Library;
using System.Threading.Tasks;

namespace Spoti_bot.Spotify.Tracks.SyncHistory
{
    public interface ISyncHistoryService
    {
        Task<int> SyncTracksFromJson(string jsonString);
    }
}