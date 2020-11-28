using SpotifyAPI.Web;
using System;
using System.Threading.Tasks;

namespace Spoti_bot.Spotify.Authorization
{
    public interface IAuthorizationService
    {
        Task<ISpotifyClient> CreateSpotifyClient();
        Uri GetLoginUri();
        Task RequestAndSaveAuthorizationToken(string code);
    }
}
