using Spoti_bot.Bot.HandleUpdate.Dto;
using Spoti_bot.Bot.Users;

namespace Spoti_bot.Spotify.Tracks.AddTrack
{
    public interface IReplyMessageService
    {
        string GetSuccessReplyMessage(UpdateDto updateDto, Track track);
        string GetExistingTrackReplyMessage(UpdateDto updateDto, Track track, User user);
    }
}
