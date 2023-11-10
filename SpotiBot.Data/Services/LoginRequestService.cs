using SpotiBot.Data.Repositories;
using SpotiBot.Library.BusinessModels.Spotify;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace SpotiBot.Data.Services
{
    public class LoginRequestService : ILoginRequestService
    {
        private readonly ILoginRequestRepository _loginRequestRepository;
        private readonly IMapper _mapper;
        private const int _loginRequestExpiresInMinutes = 10;

        public LoginRequestService(ILoginRequestRepository loginRequestRepository, IMapper mapper)
        {
            _loginRequestRepository = loginRequestRepository;
            _mapper = mapper;
        }

        public async Task<LoginRequest> Create(int reason, long userId, long? groupChatId, long privateChatId, string? trackId = null)
        {
            return _mapper.Map(await _loginRequestRepository.Upsert(new Models.LoginRequest
            {
                UserId = userId,
                Id = Guid.NewGuid().ToString(),
                ExpiresAt = DateTimeOffset.UtcNow.AddMinutes(_loginRequestExpiresInMinutes),
                GroupChatId = groupChatId,
                PrivateChatId = privateChatId,
                TrackId = trackId,
                Reason = reason
            }));
        }

        public async Task<LoginRequest?> Get(string id)
        {
            // First delete expired login requests.
            await DeleteAllExpired();

            var loginRequest = await _loginRequestRepository.Get(id);

            return loginRequest != null
                ? _mapper.Map(loginRequest)
                : null;
        }

        public Task Delete(LoginRequest loginRequest)
        {
            var loginRequestModel = _mapper.Map(loginRequest);

            return _loginRequestRepository.Delete(loginRequestModel);
        }

        private async Task DeleteAllExpired()
        {
            var expiredLoginRequests = await _loginRequestRepository.GetAllExpired();

            if (expiredLoginRequests.Any())
                await _loginRequestRepository.Delete(expiredLoginRequests);
        }
    }
}
