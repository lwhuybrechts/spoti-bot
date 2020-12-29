using System.Threading.Tasks;

namespace Spoti_bot.Spotify.Authorization
{
    public interface ILoginRequestService
    {
        Task<LoginRequest> Create(long userId, long? groupChatId, long privateChatId);
        Task<LoginRequest> Get(string id);
        Task Delete(LoginRequest loginRequest);
    }
}