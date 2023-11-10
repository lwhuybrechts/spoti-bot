using SpotiBot.Library;
using System.Threading.Tasks;

namespace SpotiBot.Api.Spotify.Tracks.SyncHistory
{
    public interface ISyncHistoryService
    {
        Task<int> SyncTracksFromJson(string jsonString);
    }
}