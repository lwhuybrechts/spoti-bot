using SpotifyAPI.Web;
using System;
using System.Threading.Tasks;

namespace Spoti_bot.Spotify.Authorization
{
    public interface IAuthorizationService
    {
        Task<Uri> CreateLoginRequest(long userId);
        Task RequestAndSaveAuthorizationToken(string code, string loginRequestId);
    }
}
