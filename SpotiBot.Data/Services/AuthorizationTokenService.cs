using SpotiBot.Data.Repositories;
using SpotiBot.Library.BusinessModels.Spotify;
using System.Threading.Tasks;

namespace SpotiBot.Data.Services
{
    public class AuthorizationTokenService : IAuthorizationTokenService
    {
        private readonly IAuthorizationTokenRepository _authorizationTokenRepository;
        private readonly IMapper _mapper;

        public AuthorizationTokenService(IAuthorizationTokenRepository authorizationTokenRepository, IMapper mapper)
        {
            _authorizationTokenRepository = authorizationTokenRepository;
            _mapper = mapper;
        }

        public async Task<AuthorizationToken?> Get(long id)
        {
            var authorizationToken = await _authorizationTokenRepository.Get(id);

            return authorizationToken != null
                ? _mapper.Map(authorizationToken)
                : null;
        }

        public async Task<AuthorizationToken> Upsert(AuthorizationToken token)
        {
            return _mapper.Map(await _authorizationTokenRepository.Upsert(_mapper.Map(token)));
        }
    }
}
