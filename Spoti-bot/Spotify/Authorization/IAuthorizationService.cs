using System;
using System.Threading.Tasks;

namespace Spoti_bot.Spotify.Authorization
{
    public interface IAuthorizationService
    {
        Task<Uri> CreateLoginRequest(long userId, LoginRequestReason reason, long? groupChatId, long privateChatId, string trackId = null);
        Task<LoginRequest> RequestAndSaveAuthorizationToken(string code, string loginRequestId);
    }
}
