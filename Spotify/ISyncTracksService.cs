using System.Threading.Tasks;

namespace Spoti_bot.Spotify
{
    public interface ISyncTracksService
    {
        Task SyncTracks();
    }
}