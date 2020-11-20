using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace Spoti_bot.Spotify
{
    public interface ISpotifyAddTrackService
    {
        Task<bool> TryAddTrackToPlaylist(Message message);
    }
}
