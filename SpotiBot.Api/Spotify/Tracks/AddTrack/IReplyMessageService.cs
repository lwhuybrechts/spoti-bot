using SpotiBot.Api.Bot.HandleUpdate.Dto;
using SpotiBot.Library.BusinessModels.Bot;
using SpotiBot.Library.BusinessModels.Spotify;

namespace SpotiBot.Api.Spotify.Tracks.AddTrack
{
    public interface IReplyMessageService
    {
        string GetSuccessReplyMessage(UpdateDto updateDto, Track track);
        string GetExistingTrackReplyMessage(UpdateDto updateDto, Track track, User user);
    }
}
