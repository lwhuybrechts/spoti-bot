using System.Threading.Tasks;

namespace Spoti_bot.Spotify.Tracks.SyncTracks
{
    public interface ISyncTracksService
    {
        Task SyncTracks();
    }
}