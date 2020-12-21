using System;
using System.Linq;
using System.Threading.Tasks;

namespace Spoti_bot.Spotify.Authorization
{
    public class LoginRequestService : ILoginRequestService
    {
        private readonly ILoginRequestRepository _loginRequestRepository;

        private const int _loginRequestExpiresInMinutes = 10;

        public LoginRequestService(ILoginRequestRepository loginRequestRepository)
        {
            _loginRequestRepository = loginRequestRepository;
        }

        public Task<LoginRequest> Create(long userId, long chatId)
        {
            return _loginRequestRepository.Upsert(new LoginRequest
            {
                UserId = userId,
                Id = Guid.NewGuid().ToString(),
                ExpiresAt = DateTimeOffset.UtcNow.AddMinutes(_loginRequestExpiresInMinutes),
                ChatId = chatId
            });
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
