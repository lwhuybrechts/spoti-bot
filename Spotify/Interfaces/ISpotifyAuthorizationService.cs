using SpotifyAPI.Web;
using System;
using System.Threading.Tasks;

namespace Spoti_bot.Spotify.Interfaces
{
    public interface ISpotifyAuthorizationService
    {
        Task<ISpotifyClient> CreateSpotifyClient();
        Uri GetLoginUri();
        Task RequestAndSaveAuthorizationToken(string code);
    }
}
