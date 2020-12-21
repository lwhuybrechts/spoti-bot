using Spoti_bot.Bot.HandleUpdate.Dto;

namespace Spoti_bot.Spotify.Tracks.AddTrack
{
    public interface ISuccessResponseService
    {
        string GetSuccessResponseText(UpdateDto updateDto, Track track);
    }
}
