using SpotiBot.Bot.HandleUpdate.Dto;
using SpotiBot.Bot.Users;

namespace SpotiBot.Spotify.Tracks.AddTrack
{
    public interface IReplyMessageService
    {
        string GetSuccessReplyMessage(UpdateDto updateDto, Track track);
        string GetExistingTrackReplyMessage(UpdateDto updateDto, Track track, User user);
    }
}
