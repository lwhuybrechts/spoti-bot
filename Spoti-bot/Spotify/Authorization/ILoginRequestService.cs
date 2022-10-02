using System.Threading.Tasks;

namespace SpotiBot.Spotify.Authorization
{
    public interface ILoginRequestService
    {
        Task<LoginRequest> Create(LoginRequestReason reason, long userId, long? groupChatId, long privateChatId, string trackId = null);
        Task<LoginRequest> Get(string id);
        Task Delete(LoginRequest loginRequest);
    }
}