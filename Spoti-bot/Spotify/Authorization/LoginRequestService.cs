using System;
using System.Linq;
using System.Threading.Tasks;

namespace SpotiBot.Spotify.Authorization
{
    public class LoginRequestService : ILoginRequestService
    {
        private readonly ILoginRequestRepository _loginRequestRepository;

        private const int _loginRequestExpiresInMinutes = 10;

        public LoginRequestService(ILoginRequestRepository loginRequestRepository)
        {
            _loginRequestRepository = loginRequestRepository;
        }

        public async Task<LoginRequest> Create(LoginRequestReason reason, long userId, long? groupChatId, long privateChatId, string trackId = null)
        {
            var loginRequest = new LoginRequest
            {
                UserId = userId,
                Id = Guid.NewGuid().ToString(),
                ExpiresAt = DateTimeOffset.UtcNow.AddMinutes(_loginRequestExpiresInMinutes),
                GroupChatId = groupChatId,
                PrivateChatId = privateChatId,
                TrackId = trackId,
                Reason = reason
            };
            
            await _loginRequestRepository.Upsert(loginRequest);

            return await _loginRequestRepository.Get(loginRequest);
        }

        public async Task<LoginRequest> Get(string id)
        {
            // First delete expired login requests.
            await DeleteAllExpired();

            return await _loginRequestRepository.Get(id);
        }

        public Task Delete(LoginRequest loginRequest)
        {
            return _loginRequestRepository.Delete(loginRequest);
        }

        private async Task DeleteAllExpired()
        {
            var expiredLoginRequests = await _loginRequestRepository.GetAllExpired();

            if (expiredLoginRequests.Any())
                await _loginRequestRepository.Delete(expiredLoginRequests);
        }
    }
}
