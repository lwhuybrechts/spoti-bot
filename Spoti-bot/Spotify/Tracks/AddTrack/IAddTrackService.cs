using Spoti_bot.Bot.HandleUpdate.Dto;
using Spoti_bot.Library;
using System.Threading.Tasks;

namespace Spoti_bot.Spotify.Tracks.AddTrack
{
    public interface IAddTrackService
    {
        Task<BotResponseCode> TryAddTrackToPlaylist(UpdateDto updateDto);
    }
}