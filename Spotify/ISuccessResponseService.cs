using Spoti_bot.Spotify.Data.Tracks;
using Telegram.Bot.Types;

namespace Spoti_bot.Spotify
{
    public interface ISuccessResponseService
    {
        string GetSuccessResponseText(Message message, Track track);
    }
}
