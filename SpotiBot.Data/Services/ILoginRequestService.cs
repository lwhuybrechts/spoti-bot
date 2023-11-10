using SpotiBot.Library.BusinessModels.Spotify;
using System.Threading.Tasks;

namespace SpotiBot.Data.Services
{
    public interface ILoginRequestService
    {
        Task<LoginRequest> Create(int reason, long userId, long? groupChatId, long privateChatId, string? trackId = null);
        Task<LoginRequest?> Get(string id);
        Task Delete(LoginRequest loginRequest);
    }
}