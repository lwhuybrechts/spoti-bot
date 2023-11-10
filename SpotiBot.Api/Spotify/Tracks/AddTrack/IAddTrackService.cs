using SpotiBot.Api.Bot.HandleUpdate.Dto;
using SpotiBot.Api.Library;
using System.Threading.Tasks;

namespace SpotiBot.Api.Spotify.Tracks.AddTrack
{
    public interface IAddTrackService
    {
        Task<BotResponseCode> TryAddTrackToPlaylist(UpdateDto updateDto);
    }
}