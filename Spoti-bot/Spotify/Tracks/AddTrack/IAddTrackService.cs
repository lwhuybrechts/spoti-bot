using Spoti_bot.Library;
using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace Spoti_bot.Spotify.Tracks.AddTrack
{
    public interface IAddTrackService
    {
        Task<BotResponseCode> TryAddTrackToPlaylist(Message message, Bot.Chats.Chat chat);
    }
}
