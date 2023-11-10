using SpotiBot.Library.BusinessModels.Bot;
using System.Threading.Tasks;

namespace SpotiBot.Api.Spotify.Tracks.SyncTracks
{
    public interface ISyncTracksService
    {
        Task SyncTracks(Chat chat, bool shouldUpdateExistingTracks = false);
    }
}