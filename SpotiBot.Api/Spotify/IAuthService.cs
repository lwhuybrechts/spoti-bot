using SpotifyAPI.Web;
using System.Threading.Tasks;

namespace SpotiBot.Api.Spotify
{
    public interface IAuthService
    {
        Task<ISpotifyClient> GetClient(long userId);
    }
}