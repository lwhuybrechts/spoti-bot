using SpotiBot.Bot.HandleUpdate.Dto;
using SpotiBot.Library;
using System.Threading.Tasks;

namespace SpotiBot.Spotify.Tracks.AddTrack
{
    public interface IAddTrackService
    {
        Task<BotResponseCode> TryAddTrackToPlaylist(UpdateDto updateDto);
    }
}