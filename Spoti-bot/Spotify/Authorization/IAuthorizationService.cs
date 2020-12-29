using System;
using System.Threading.Tasks;

namespace Spoti_bot.Spotify.Authorization
{
    public interface IAuthorizationService
    {
        Task<Uri> CreateLoginRequest(long userId, long? groupChatId, long privateChatId);
        Task RequestAndSaveAuthorizationToken(string code, string loginRequestId);
    }
}
