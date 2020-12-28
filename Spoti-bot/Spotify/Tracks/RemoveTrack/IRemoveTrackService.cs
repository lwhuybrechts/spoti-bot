using Spoti_bot.Bot.HandleUpdate.Dto;
using Spoti_bot.Library;
using System.Threading.Tasks;

namespace Spoti_bot.Spotify.Tracks.RemoveTrack
{
    public interface IRemoveTrackService
    {
        Task<BotResponseCode> TryRemoveTrackFromPlaylist(UpdateDto updateDto);
    }
}