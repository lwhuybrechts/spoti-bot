using SpotifyAPI.Web;
using System.Threading.Tasks;

namespace Spoti_bot.Spotify.Api
{
    public interface ISpotifyClientFactory
    {
        Task<ISpotifyClient> Create(long userId);
    }
}