using AutoMapper;
using System.Threading.Tasks;

namespace Spoti_bot.Bot.Users
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;

        public UserService(IUserRepository userRepository, IMapper mapper)
        {
            _userRepository = userRepository;
            _mapper = mapper;
        }

        public async Task<User> SaveUser(Telegram.Bot.Types.User telegramUser)
        {
            // Save the user to the storage.
            var user = _mapper.Map<User>(telegramUser);

            return await _userRepository.Upsert(user);
        }
    }
}
