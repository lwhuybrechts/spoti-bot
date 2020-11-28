using Telegram.Bot.Types;

namespace Spoti_bot.Spotify.Tracks.AddTrack
{
    public interface ISuccessResponseService
    {
        string GetSuccessResponseText(Message message, Track track);
    }
}
