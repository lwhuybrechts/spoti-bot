using SpotiBot.Library.BusinessModels.Spotify;
using SpotiBot.Library.Enums;
using System;
using System.Threading.Tasks;

namespace SpotiBot.Api.Spotify.Authorization
{
    public interface IAuthorizationService
    {
        Task<Uri> CreateLoginRequest(long userId, LoginRequestReason reason, long? groupChatId, long privateChatId, string? trackId = null);
        Task<LoginRequest> RequestAndSaveAuthorizationToken(string code, string loginRequestId);
    }
}
