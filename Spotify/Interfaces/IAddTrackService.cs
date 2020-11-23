using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace Spoti_bot.Spotify.Interfaces
{
    public interface IAddTrackService
    {
        Task<bool> TryAddTrackToPlaylist(Message message);
    }
}
