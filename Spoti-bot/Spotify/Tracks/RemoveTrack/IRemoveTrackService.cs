using SpotiBot.Bot.HandleUpdate.Dto;
using SpotiBot.Library;
using System.Threading.Tasks;

namespace SpotiBot.Spotify.Tracks.RemoveTrack
{
    public interface IRemoveTrackService
    {
        Task<BotResponseCode> TryRemoveTrackFromPlaylist(UpdateDto updateDto);
    }
}