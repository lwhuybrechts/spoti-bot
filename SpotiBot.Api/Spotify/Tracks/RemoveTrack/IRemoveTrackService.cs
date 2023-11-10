using SpotiBot.Api.Bot.HandleUpdate.Dto;
using SpotiBot.Api.Library;
using System.Threading.Tasks;

namespace SpotiBot.Api.Spotify.Tracks.RemoveTrack
{
    public interface IRemoveTrackService
    {
        Task<BotResponseCode> TryRemoveTrackFromPlaylist(UpdateDto updateDto);
    }
}